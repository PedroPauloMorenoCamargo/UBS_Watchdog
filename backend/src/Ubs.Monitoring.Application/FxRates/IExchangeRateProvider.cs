namespace Ubs.Monitoring.Application.FxRates;

/// <summary>
/// Provider interface for fetching exchange rates from external sources.
/// Abstracts the external API integration to allow for different implementations.
/// </summary>
public interface IExchangeRateProvider
{
    /// <summary>
    /// Retrieves the current exchange rate for a currency pair.
    /// Uses caching to minimize external API calls.
    /// </summary>
    /// <param name="baseCurrencyCode">The base currency code (e.g., "USD").</param>
    /// <param name="quoteCurrencyCode">The quote currency code (e.g., "BRL").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the exchange rate data and any error message.
    /// If successful, Rate contains the data and ErrorMessage is null.
    /// If failed, Rate is null and ErrorMessage contains the error description.
    /// </returns>
    Task<(ExchangeRateDto? Rate, string? ErrorMessage)> GetExchangeRateAsync(
        string baseCurrencyCode,
        string quoteCurrencyCode,
        CancellationToken ct);

    /// <summary>
    /// Retrieves all available exchange rates for a base currency.
    /// Uses caching to minimize external API calls.
    /// </summary>
    /// <param name="baseCurrencyCode">The base currency code (e.g., "USD").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing all exchange rates and any error message.
    /// If successful, Rates contains the data and ErrorMessage is null.
    /// If failed, Rates is null and ErrorMessage contains the error description.
    /// </returns>
    Task<(ExchangeRatesResponseDto? Rates, string? ErrorMessage)> GetAllRatesAsync(
        string baseCurrencyCode,
        CancellationToken ct);
}
