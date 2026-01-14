namespace Ubs.Monitoring.Application.FxRates;

/// <summary>
/// Service interface for FX rate business operations.
/// Handles currency conversion and rate persistence.
/// </summary>
public interface IFxRateService
{
    /// <summary>
    /// Gets the base currency code used for all conversions (e.g., "USD").
    /// </summary>
    string BaseCurrencyCode { get; }

    /// <summary>
    /// Converts an amount from one currency to the base currency (USD).
    /// Automatically fetches the exchange rate from external API or database,
    /// persists the rate for traceability, and returns the converted amount.
    /// </summary>
    /// <param name="amount">The amount to convert.</param>
    /// <param name="currencyCode">The source currency code (e.g., "BRL").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing:
    /// - BaseAmount: The converted amount in base currency (USD)
    /// - FxRateId: The ID of the FX rate used (null if same currency)
    /// - Error: Error message if conversion failed
    /// </returns>
    Task<(decimal BaseAmount, Guid? FxRateId, string? Error)> ConvertToBaseCurrencyAsync(
        decimal amount,
        string currencyCode,
        CancellationToken ct);
}
