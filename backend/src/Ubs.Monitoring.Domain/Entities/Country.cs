using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

/// <summary>
/// Represents a country for client registration and compliance tracking.
/// Uses ISO 3166-1 alpha-2 standard (2-letter country codes).
/// </summary>
public class Country
{
    private Country() { } // EF Core

    public Country(string code, string name, RiskLevel riskLevel = RiskLevel.Low)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Country code is required", nameof(code));
        if (code.Length != 2)
            throw new ArgumentException("Country code must be exactly 2 characters (ISO 3166-1 alpha-2)", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required", nameof(name));

        Code = code.ToUpperInvariant();
        Name = name;
        RiskLevel = riskLevel;
    }

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., BR, US, GB).
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Country name in English (e.g., Brazil, United States, United Kingdom).
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Risk level for compliance purposes (Low, Medium, High).
    /// Used by BannedCountries and other compliance rules.
    /// </summary>
    public RiskLevel RiskLevel { get; private set; }

    /// <summary>
    /// Updates the risk level of the country.
    /// </summary>
    public void UpdateRiskLevel(RiskLevel riskLevel)
    {
        RiskLevel = riskLevel;
    }
}
