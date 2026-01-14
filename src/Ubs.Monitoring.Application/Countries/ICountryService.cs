namespace Ubs.Monitoring.Application.Countries;

/// <summary>
/// Service interface for country business operations.
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Retrieves all countries for frontend dropdown lists.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of countries with code, name, and risk status.</returns>
    Task<IReadOnlyList<CountryResponseDto>> GetAllCountriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates the risk level of a country for compliance purposes.
    /// </summary>
    /// <param name="code">ISO alpha-2 country code (e.g., BR, US, GB).</param>
    /// <param name="request">Request containing the new risk level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated country if found and updated, null otherwise.</returns>
    Task<CountryResponseDto?> UpdateRiskLevelAsync(string code, UpdateCountryRiskLevelRequest request, CancellationToken ct = default);
}
