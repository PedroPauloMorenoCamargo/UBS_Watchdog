using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.FxRates;

/// <summary>
/// Repository interface for FX rate data access operations.
/// </summary>
public interface IFxRateRepository
{
    /// <summary>
    /// Retrieves the most recent exchange rate for the specified currency pair.
    /// </summary>
    /// <param name="baseCurrencyCode">The base currency code (e.g., "USD").</param>
    /// <param name="quoteCurrencyCode">The quote currency code (e.g., "BRL").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The most recent FX rate if found; otherwise, null.</returns>
    Task<FxRate?> GetLatestAsync(string baseCurrencyCode, string quoteCurrencyCode, CancellationToken ct);

    /// <summary>
    /// Retrieves an FX rate by its unique identifier.
    /// </summary>
    /// <param name="fxRateId">The unique identifier of the FX rate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The FX rate if found; otherwise, null.</returns>
    Task<FxRate?> GetByIdAsync(Guid fxRateId, CancellationToken ct);

    /// <summary>
    /// Checks if an FX rate exists for the specified currency pair.
    /// </summary>
    /// <param name="baseCurrencyCode">The base currency code.</param>
    /// <param name="quoteCurrencyCode">The quote currency code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if an FX rate exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string baseCurrencyCode, string quoteCurrencyCode, CancellationToken ct);

    /// <summary>
    /// Retrieves the most recent exchange rate for a currency pair within a time window.
    /// Used to avoid creating duplicate rates for the same pair within a short period.
    /// </summary>
    /// <param name="baseCurrencyCode">The base currency code (e.g., "USD").</param>
    /// <param name="quoteCurrencyCode">The quote currency code (e.g., "BRL").</param>
    /// <param name="asOfUtc">The reference timestamp.</param>
    /// <param name="windowMinutes">Time window in minutes to consider as "same" rate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The FX rate if found within the window; otherwise, null.</returns>
    Task<FxRate?> GetByPairAndTimeWindowAsync(
        string baseCurrencyCode,
        string quoteCurrencyCode,
        DateTimeOffset asOfUtc,
        int windowMinutes,
        CancellationToken ct);

    /// <summary>
    /// Adds a new FX rate to the database context.
    /// Call SaveChangesAsync to persist changes.
    /// </summary>
    /// <param name="fxRate">The FX rate entity to add.</param>
    void Add(FxRate fxRate);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct);
}
