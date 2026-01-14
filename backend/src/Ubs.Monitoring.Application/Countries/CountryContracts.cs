using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Countries;

/// <summary>
/// Response DTO containing country information for frontend dropdown lists.
/// </summary>
public sealed record CountryResponseDto(
    string Code,
    string Name,
    RiskLevel RiskLevel
);

/// <summary>
/// Request DTO for updating a country's risk level.
/// </summary>
public sealed record UpdateCountryRiskLevelRequest(
    RiskLevel NewRiskLevel
);
