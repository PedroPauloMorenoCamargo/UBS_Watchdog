using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.FxRates;

public sealed class FxRateService : IFxRateService
{
    private readonly IFxRateRepository _fxRateRepository;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly FxRateServiceOptions _options;
    private readonly ILogger<FxRateService> _logger;

    public FxRateService(
        IFxRateRepository fxRateRepository,
        IExchangeRateProvider exchangeRateProvider,
        IOptions<FxRateServiceOptions> options,
        ILogger<FxRateService> logger)
    {
        _fxRateRepository = fxRateRepository;
        _exchangeRateProvider = exchangeRateProvider;
        _options = options.Value;
        _logger = logger;
    }

    public string BaseCurrencyCode => _options.BaseCurrencyCode;

    public async Task<(decimal BaseAmount, Guid? FxRateId, string? Error)> ConvertToBaseCurrencyAsync(
        decimal amount,
        string currencyCode,
        CancellationToken ct)
    {
        var normalizedCurrency = currencyCode.Trim().ToUpperInvariant();
        var baseCurrency = _options.BaseCurrencyCode;

        // If same currency, no conversion needed
        if (normalizedCurrency == baseCurrency)
        {
            _logger.LogDebug("No conversion needed: {Currency} is the base currency", normalizedCurrency);
            return (amount, null, null);
        }

        // Try to get exchange rate from external API (with cache and fallback)
        var (exchangeRate, errorMessage) = await _exchangeRateProvider.GetExchangeRateAsync(
            baseCurrency, normalizedCurrency, ct);

        if (exchangeRate is not null)
        {
            // Persist the rate to database for traceability
            var fxRateId = await GetOrCreateFxRateAsync(exchangeRate, ct);

            // Convert: baseAmount = amount / rate
            // Example: 1000 BRL / 5.50 (USD/BRL rate) = 181.82 USD
            var baseAmount = Math.Round(amount / exchangeRate.Rate, 2, MidpointRounding.AwayFromZero);

            _logger.LogDebug("Currency conversion via API: {Amount} {Currency} -> {BaseAmount} {BaseCurrency} (rate: {Rate}, fxRateId: {FxRateId})",
                amount, normalizedCurrency, baseAmount, baseCurrency, exchangeRate.Rate, fxRateId);

            return (baseAmount, fxRateId, null);
        }

        // Fallback: Try to get rate from database (legacy behavior)
        var dbFxRate = await _fxRateRepository.GetLatestAsync(baseCurrency, normalizedCurrency, ct);

        if (dbFxRate is not null)
        {
            var baseAmount = Math.Round(amount / dbFxRate.Rate, 2, MidpointRounding.AwayFromZero);

            _logger.LogDebug("Currency conversion via DB fallback: {Amount} {Currency} -> {BaseAmount} {BaseCurrency} (rate: {Rate})",
                amount, normalizedCurrency, baseAmount, baseCurrency, dbFxRate.Rate);

            return (baseAmount, dbFxRate.Id, null);
        }

        return (0, null, $"No exchange rate available for {normalizedCurrency} to {baseCurrency}. {errorMessage}");
    }

    /// <summary>
    /// Reuses existing FX rate within time window, or creates new from API response.
    /// </summary>
    private async Task<Guid> GetOrCreateFxRateAsync(ExchangeRateDto exchangeRate, CancellationToken ct)
    {
        // Check if we already have a rate for this pair within the time window
        var existingRate = await _fxRateRepository.GetByPairAndTimeWindowAsync(
            exchangeRate.BaseCurrencyCode,
            exchangeRate.QuoteCurrencyCode,
            exchangeRate.LastUpdatedUtc,
            _options.RateReuseWindowMinutes,
            ct);

        if (existingRate is not null)
        {
            _logger.LogDebug("Reusing existing FX rate {FxRateId} for {Base}/{Quote}",
                existingRate.Id, exchangeRate.BaseCurrencyCode, exchangeRate.QuoteCurrencyCode);
            return existingRate.Id;
        }

        // Create new FX rate from API data
        var newFxRate = new FxRate(
            exchangeRate.BaseCurrencyCode,
            exchangeRate.QuoteCurrencyCode,
            exchangeRate.Rate,
            exchangeRate.LastUpdatedUtc
        );

        _fxRateRepository.Add(newFxRate);
        await _fxRateRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Created new FX rate {FxRateId}: {Base}/{Quote} = {Rate} as of {AsOf}",
            newFxRate.Id, exchangeRate.BaseCurrencyCode, exchangeRate.QuoteCurrencyCode,
            exchangeRate.Rate, exchangeRate.LastUpdatedUtc);

        return newFxRate.Id;
    }
}
