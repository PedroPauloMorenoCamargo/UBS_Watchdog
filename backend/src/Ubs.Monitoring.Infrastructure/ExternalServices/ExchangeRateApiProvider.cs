using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ubs.Monitoring.Application.FxRates;

namespace Ubs.Monitoring.Infrastructure.ExternalServices;

/// <summary>
/// Implementation of exchange rate provider using ExchangeRate-API.
/// Features:
/// - In-memory caching to minimize API calls and costs
/// - Retry policy with exponential backoff for transient failures
/// - Database fallback when API is unavailable
/// - Comprehensive error handling and logging
/// </summary>
public sealed class ExchangeRateApiProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IFxRateRepository _fxRateRepository;
    private readonly ExchangeRateApiOptions _options;
    private readonly ILogger<ExchangeRateApiProvider> _logger;

    private const string CacheKeyPrefix = "ExchangeRate";

    public ExchangeRateApiProvider(
        HttpClient httpClient,
        IMemoryCache cache,
        IFxRateRepository fxRateRepository,
        IOptions<ExchangeRateApiOptions> options,
        ILogger<ExchangeRateApiProvider> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _fxRateRepository = fxRateRepository;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the exchange rate for a specific currency pair.
    /// </summary>
    public async Task<(ExchangeRateDto? Rate, string? ErrorMessage)> GetExchangeRateAsync(
        string baseCurrencyCode,
        string quoteCurrencyCode,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(baseCurrencyCode);
        ArgumentNullException.ThrowIfNull(quoteCurrencyCode);

        var normalizedBase = baseCurrencyCode.Trim().ToUpperInvariant();
        var normalizedQuote = quoteCurrencyCode.Trim().ToUpperInvariant();

        _logger.LogDebug("Getting exchange rate for {Base}/{Quote}", normalizedBase, normalizedQuote);

        // Check cache first
        var cacheKey = GetPairCacheKey(normalizedBase, normalizedQuote);
        if (_cache.TryGetValue(cacheKey, out ExchangeRateDto? cachedRate) && cachedRate is not null)
        {
            _logger.LogDebug("Cache hit for {Base}/{Quote}", normalizedBase, normalizedQuote);
            return (cachedRate, null);
        }

        // Try to get all rates for base currency (more efficient)
        var (allRates, errorMessage) = await GetAllRatesAsync(normalizedBase, ct);

        if (allRates is not null && allRates.ConversionRates.TryGetValue(normalizedQuote, out var rate))
        {
            var exchangeRate = new ExchangeRateDto(
                normalizedBase,
                normalizedQuote,
                rate,
                allRates.LastUpdatedUtc,
                allRates.NextUpdateUtc
            );

            // Cache individual pair
            CacheRate(cacheKey, exchangeRate);

            return (exchangeRate, null);
        }

        // If quote currency not found in the rates
        if (allRates is not null)
        {
            errorMessage = $"Currency code '{normalizedQuote}' is not supported by ExchangeRate-API.";
            _logger.LogWarning("Currency {Quote} not found in rates for base {Base}", normalizedQuote, normalizedBase);
        }

        // Fallback to database
        if (_options.UseDatabaseFallback)
        {
            var dbRate = await GetFallbackFromDatabaseAsync(normalizedBase, normalizedQuote, ct);
            if (dbRate is not null)
            {
                return (dbRate, null);
            }
        }

        return (null, errorMessage ?? "Failed to retrieve exchange rate.");
    }

    /// <summary>
    /// Retrieves all exchange rates for a base currency.
    /// </summary>
    public async Task<(ExchangeRatesResponseDto? Rates, string? ErrorMessage)> GetAllRatesAsync(
        string baseCurrencyCode,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(baseCurrencyCode);

        var normalizedBase = baseCurrencyCode.Trim().ToUpperInvariant();

        // Check cache first
        var cacheKey = GetAllRatesCacheKey(normalizedBase);
        if (_cache.TryGetValue(cacheKey, out ExchangeRatesResponseDto? cachedRates) && cachedRates is not null)
        {
            _logger.LogDebug("Cache hit for all rates with base {Base}", normalizedBase);
            return (cachedRates, null);
        }

        // Fetch from API with retry
        return await FetchFromApiWithRetryAsync(normalizedBase, ct);
    }

    #region Private Methods - API Calls

    /// <summary>
    /// Fetches rates from ExchangeRate-API with retry logic.
    /// </summary>
    private async Task<(ExchangeRatesResponseDto? Rates, string? ErrorMessage)> FetchFromApiWithRetryAsync(
        string baseCurrencyCode,
        CancellationToken ct)
    {
        var lastError = string.Empty;

        for (int attempt = 0; attempt < _options.MaxRetryAttempts; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    var delay = CalculateRetryDelay(attempt);
                    _logger.LogWarning("Retry attempt {Attempt} for ExchangeRate-API after {Delay}ms delay",
                        attempt + 1, delay);
                    await Task.Delay(delay, ct);
                }

                var result = await FetchFromApiAsync(baseCurrencyCode, ct);

                if (result.Rates is not null)
                {
                    return result;
                }

                lastError = result.ErrorMessage ?? "Unknown error";

                // Don't retry for non-transient errors
                if (IsNonTransientError(lastError))
                {
                    _logger.LogWarning("Non-transient error from ExchangeRate-API: {Error}", lastError);
                    break;
                }
            }
            catch (TaskCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                lastError = $"HTTP error: {ex.Message}";
                _logger.LogWarning(ex, "HTTP error on attempt {Attempt} for ExchangeRate-API", attempt + 1);
            }
            catch (Exception ex)
            {
                lastError = $"Unexpected error: {ex.Message}";
                _logger.LogError(ex, "Unexpected error on attempt {Attempt} for ExchangeRate-API", attempt + 1);
                break; // Don't retry unexpected errors
            }
        }

        _logger.LogError("All retry attempts exhausted for ExchangeRate-API. Last error: {Error}", lastError);
        return (null, lastError);
    }

    /// <summary>
    /// Makes a single API call to ExchangeRate-API.
    /// </summary>
    private async Task<(ExchangeRatesResponseDto? Rates, string? ErrorMessage)> FetchFromApiAsync(
        string baseCurrencyCode,
        CancellationToken ct)
    {
        var url = $"{_options.BaseUrl}/{_options.ApiKey}/latest/{baseCurrencyCode}";
        _logger.LogDebug("Calling ExchangeRate-API: GET /latest/{Base}", baseCurrencyCode);

        using var response = await _httpClient.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            _logger.LogWarning("ExchangeRate-API returned HTTP {StatusCode}", statusCode);

            return statusCode switch
            {
                HttpStatusCode.Unauthorized => (null, "Invalid API key. Please check your ExchangeRate-API configuration."),
                HttpStatusCode.TooManyRequests => (null, "API quota reached. Please upgrade your plan or wait for quota reset."),
                HttpStatusCode.NotFound => (null, $"Currency code '{baseCurrencyCode}' not found."),
                _ => (null, $"ExchangeRate-API error: HTTP {(int)statusCode}")
            };
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>(ct);

        if (apiResponse is null)
        {
            return (null, "Failed to parse ExchangeRate-API response.");
        }

        if (apiResponse.Result != "success")
        {
            var errorMessage = MapApiErrorToMessage(apiResponse.Error_type);
            _logger.LogWarning("ExchangeRate-API error: {ErrorType}", apiResponse.Error_type);
            return (null, errorMessage);
        }

        // Transform to our DTO
        var rates = TransformApiResponse(apiResponse);

        // Cache the result
        CacheAllRates(baseCurrencyCode, rates);

        _logger.LogInformation("Successfully fetched {Count} exchange rates for base {Base} from ExchangeRate-API",
            rates.ConversionRates.Count, baseCurrencyCode);

        return (rates, null);
    }

    #endregion

    #region Private Methods - Transformation & Mapping

    /// <summary>
    /// Transforms the raw API response to our internal DTO.
    /// </summary>
    private static ExchangeRatesResponseDto TransformApiResponse(ExchangeRateApiResponse response)
    {
        var lastUpdated = DateTimeOffset.FromUnixTimeSeconds(response.Time_last_update_unix);
        var nextUpdate = DateTimeOffset.FromUnixTimeSeconds(response.Time_next_update_unix);

        return new ExchangeRatesResponseDto(
            response.Base_code.ToUpperInvariant(),
            lastUpdated,
            nextUpdate,
            response.Conversion_rates.ToDictionary(
                kvp => kvp.Key.ToUpperInvariant(),
                kvp => kvp.Value
            )
        );
    }

    /// <summary>
    /// Maps API error types to user-friendly messages.
    /// </summary>
    private static string MapApiErrorToMessage(string? errorType)
    {
        return errorType switch
        {
            "unsupported-code" => "The specified currency code is not supported.",
            "malformed-request" => "Invalid request format.",
            "invalid-key" => "Invalid API key. Please check your ExchangeRate-API configuration.",
            "inactive-account" => "API account is inactive. Please confirm your email address.",
            "quota-reached" => "API quota reached. Please upgrade your plan or wait for quota reset.",
            _ => $"ExchangeRate-API error: {errorType ?? "unknown"}"
        };
    }

    /// <summary>
    /// Determines if an error is non-transient (should not be retried).
    /// </summary>
    private static bool IsNonTransientError(string errorMessage)
    {
        return errorMessage.Contains("Invalid API key", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("not supported", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("quota reached", StringComparison.OrdinalIgnoreCase) ||
               errorMessage.Contains("inactive account", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Private Methods - Caching

    /// <summary>
    /// Caches all rates for a base currency.
    /// </summary>
    private void CacheAllRates(string baseCurrencyCode, ExchangeRatesResponseDto rates)
    {
        var cacheKey = GetAllRatesCacheKey(baseCurrencyCode);
        var expiration = TimeSpan.FromMinutes(_options.CacheMinutes);

        _cache.Set(cacheKey, rates, expiration);

        _logger.LogDebug("Cached all rates for base {Base} (expires in {Minutes} minutes)",
            baseCurrencyCode, _options.CacheMinutes);
    }

    /// <summary>
    /// Caches a single exchange rate pair.
    /// </summary>
    private void CacheRate(string cacheKey, ExchangeRateDto rate)
    {
        var expiration = TimeSpan.FromMinutes(_options.CacheMinutes);
        _cache.Set(cacheKey, rate, expiration);
    }

    private static string GetAllRatesCacheKey(string baseCurrencyCode)
        => $"{CacheKeyPrefix}_All_{baseCurrencyCode}";

    private static string GetPairCacheKey(string baseCurrencyCode, string quoteCurrencyCode)
        => $"{CacheKeyPrefix}_{baseCurrencyCode}_{quoteCurrencyCode}";

    #endregion

    #region Private Methods - Database Fallback

    /// <summary>
    /// Retrieves exchange rate from database as fallback.
    /// </summary>
    private async Task<ExchangeRateDto?> GetFallbackFromDatabaseAsync(
        string baseCurrencyCode,
        string quoteCurrencyCode,
        CancellationToken ct)
    {
        _logger.LogInformation("Falling back to database for {Base}/{Quote} exchange rate",
            baseCurrencyCode, quoteCurrencyCode);

        var dbRate = await _fxRateRepository.GetLatestAsync(baseCurrencyCode, quoteCurrencyCode, ct);

        if (dbRate is not null)
        {
            _logger.LogInformation("Found fallback rate in database: {Rate} (as of {Date})",
                dbRate.Rate, dbRate.AsOfUtc);

            return new ExchangeRateDto(
                dbRate.BaseCurrencyCode,
                dbRate.QuoteCurrencyCode,
                dbRate.Rate,
                dbRate.AsOfUtc,
                dbRate.AsOfUtc.AddDays(1) // Assume next update is next day
            );
        }

        _logger.LogWarning("No fallback rate found in database for {Base}/{Quote}",
            baseCurrencyCode, quoteCurrencyCode);

        return null;
    }

    #endregion

    #region Private Methods - Retry Logic

    /// <summary>
    /// Calculates retry delay with exponential backoff.
    /// </summary>
    private int CalculateRetryDelay(int attemptNumber)
    {
        // Exponential backoff: baseDelay * 2^attempt
        return _options.BaseRetryDelayMs * (int)Math.Pow(2, attemptNumber);
    }

    #endregion
}
