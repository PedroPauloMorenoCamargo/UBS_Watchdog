using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Countries;

/// <summary>
/// Repository interface for country data access operations.
/// </summary>
public interface ICountryRepository
{
    /// <summary>
    /// Retrieves all countries from the database, ordered by name.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all countries.</returns>
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a country by its ISO 3166-1 alpha-2 code (read-only).
    /// </summary>
    /// <param name="code">ISO alpha-2 country code (e.g., BR, US, GB).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Country if found, null otherwise.</returns>
    Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a tracked country by its ISO 3166-1 alpha-2 code for update operations.
    /// </summary>
    /// <param name="code">ISO alpha-2 country code (e.g., BR, US, GB).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tracked country if found, null otherwise.</returns>
    Task<Country?> GetByCodeForUpdateAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Checks if a country exists by its ISO 3166-1 alpha-2 code.
    /// </summary>
    /// <param name="code">ISO alpha-2 country code (e.g., BR, US, GB).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if country exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a set of existing country codes from a collection of codes.
    /// This method performs a single database query instead of multiple individual checks,
    /// optimizing the N+1 query problem when validating multiple country codes.
    /// </summary>
    /// <param name="codes">Collection of country codes to check (e.g., BR, US, GB).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HashSet containing only the codes that exist in the database (normalized to uppercase).</returns>
    Task<HashSet<string>> GetExistingCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);

    /// <summary>
    /// Persists changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct = default);
}
