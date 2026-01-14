using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Countries;

public sealed class CountryService : ICountryService
{
    private readonly ICountryRepository _countries;
    private readonly ILogger<CountryService> _logger;

    public CountryService(ICountryRepository countries, ILogger<CountryService> logger)
    {
        _countries = countries;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CountryResponseDto>> GetAllCountriesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all countries for dropdown list");

        var countries = await _countries.GetAllAsync(ct);

        var dtos = countries.Select(MapToResponseDto).ToList();

        _logger.LogInformation("Returning {Count} countries to client", dtos.Count);

        return dtos;
    }

    public async Task<CountryResponseDto?> UpdateRiskLevelAsync(string code, UpdateCountryRiskLevelRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Updating risk level for country {Code} to {RiskLevel}", code, request.NewRiskLevel);

        var country = await _countries.GetByCodeForUpdateAsync(code, ct);

        if (country == null)
        {
            _logger.LogWarning("Country with code {Code} not found", code);
            return null;
        }

        country.UpdateRiskLevel(request.NewRiskLevel);

        await _countries.SaveChangesAsync(ct);

        _logger.LogInformation("Risk level updated successfully for country {Code}", code);

        return MapToResponseDto(country);
    }

    private static CountryResponseDto MapToResponseDto(Country country)
    {
        return new CountryResponseDto(
            Code: country.Code,
            Name: country.Name,
            RiskLevel: country.RiskLevel
        );
    }
}
