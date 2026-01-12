using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.FxRates;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for exchange rate operations.
/// Provides real-time exchange rates via ExchangeRate-API integration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly IFxRateRepository _fxRateRepository;
    private readonly ILogger<ExchangeRatesController> _logger;

    public ExchangeRatesController(
        IExchangeRateProvider exchangeRateProvider,
        IFxRateRepository fxRateRepository,
        ILogger<ExchangeRatesController> logger)
    {
        _exchangeRateProvider = exchangeRateProvider;
        _fxRateRepository = fxRateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current exchange rate for a currency pair.
    /// </summary>
    /// <param name="baseCurrency">Base currency code (e.g., USD).</param>
    /// <param name="quoteCurrency">Quote currency code (e.g., BRL).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current exchange rate.</returns>
    /// <response code="200">Returns the exchange rate.</response>
    /// <response code="400">If the currency codes are invalid.</response>
    /// <response code="404">If no exchange rate is available.</response>
    /// <response code="503">If the exchange rate service is unavailable.</response>
    [HttpGet("{baseCurrency}/{quoteCurrency}")]
    [ProducesResponseType(typeof(ExchangeRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetExchangeRate(
        [FromRoute] string baseCurrency,
        [FromRoute] string quoteCurrency,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || baseCurrency.Length != 3)
        {
            return BadRequest("Base currency must be a valid 3-letter ISO currency code.");
        }

        if (string.IsNullOrWhiteSpace(quoteCurrency) || quoteCurrency.Length != 3)
        {
            return BadRequest("Quote currency must be a valid 3-letter ISO currency code.");
        }

        _logger.LogInformation("Getting exchange rate for {Base}/{Quote}",
            baseCurrency.ToUpperInvariant(), quoteCurrency.ToUpperInvariant());

        var (rate, errorMessage) = await _exchangeRateProvider.GetExchangeRateAsync(
            baseCurrency, quoteCurrency, ct);

        if (rate is not null)
        {
            return Ok(rate);
        }

        // Check if it's a "not found" error vs service unavailable
        if (errorMessage?.Contains("not supported", StringComparison.OrdinalIgnoreCase) == true ||
            errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
        {
            return NotFound(new { error = errorMessage });
        }

        _logger.LogWarning("Exchange rate service error: {Error}", errorMessage);
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = errorMessage });
    }

    /// <summary>
    /// Gets all available exchange rates for a base currency.
    /// </summary>
    /// <param name="baseCurrency">Base currency code (e.g., USD).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All exchange rates for the base currency.</returns>
    /// <response code="200">Returns all exchange rates.</response>
    /// <response code="400">If the currency code is invalid.</response>
    /// <response code="503">If the exchange rate service is unavailable.</response>
    [HttpGet("{baseCurrency}")]
    [ProducesResponseType(typeof(ExchangeRatesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAllRates(
        [FromRoute] string baseCurrency,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || baseCurrency.Length != 3)
        {
            return BadRequest("Base currency must be a valid 3-letter ISO currency code.");
        }

        _logger.LogInformation("Getting all exchange rates for base {Base}",
            baseCurrency.ToUpperInvariant());

        var (rates, errorMessage) = await _exchangeRateProvider.GetAllRatesAsync(baseCurrency, ct);

        if (rates is not null)
        {
            return Ok(rates);
        }

        _logger.LogWarning("Exchange rate service error: {Error}", errorMessage);
        return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = errorMessage });
    }

    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    /// <param name="request">The conversion request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The converted amount with exchange rate details.</returns>
    /// <response code="200">Returns the conversion result.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="503">If the exchange rate service is unavailable.</response>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(CurrencyConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ConvertCurrency(
        [FromBody] CurrencyConversionRequest request,
        CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.FromCurrency) || request.FromCurrency.Length != 3)
        {
            return BadRequest("FromCurrency must be a valid 3-letter ISO currency code.");
        }

        if (string.IsNullOrWhiteSpace(request.ToCurrency) || request.ToCurrency.Length != 3)
        {
            return BadRequest("ToCurrency must be a valid 3-letter ISO currency code.");
        }

        _logger.LogInformation("Converting {Amount} from {From} to {To}",
            request.Amount, request.FromCurrency.ToUpperInvariant(), request.ToCurrency.ToUpperInvariant());

        var (rate, errorMessage) = await _exchangeRateProvider.GetExchangeRateAsync(
            request.FromCurrency, request.ToCurrency, ct);

        if (rate is null)
        {
            _logger.LogWarning("Exchange rate service error during conversion: {Error}", errorMessage);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = errorMessage });
        }

        var convertedAmount = Math.Round(request.Amount * rate.Rate, 2, MidpointRounding.AwayFromZero);

        var response = new CurrencyConversionResponse(
            FromCurrency: rate.BaseCurrencyCode,
            ToCurrency: rate.QuoteCurrencyCode,
            OriginalAmount: request.Amount,
            ConvertedAmount: convertedAmount,
            ExchangeRate: rate.Rate,
            LastUpdatedUtc: rate.LastUpdatedUtc
        );

        return Ok(response);
    }
}

#region Request/Response DTOs

/// <summary>
/// Request DTO for currency conversion.
/// </summary>
public sealed record CurrencyConversionRequest(
    decimal Amount,
    string FromCurrency,
    string ToCurrency
);

/// <summary>
/// Response DTO for currency conversion.
/// </summary>
public sealed record CurrencyConversionResponse(
    string FromCurrency,
    string ToCurrency,
    decimal OriginalAmount,
    decimal ConvertedAmount,
    decimal ExchangeRate,
    DateTimeOffset LastUpdatedUtc
);

#endregion
