using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Represents a row from CSV/Excel import file.
/// </summary>
public sealed class ClientImportRow
{
    public string LegalType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? RiskLevel { get; set; }

    /// <summary>
    /// Converts the import row to a CreateClientRequest.
    /// </summary>
    public CreateClientRequest ToRequest()
    {
        // Parse LegalType enum
        if (!Enum.TryParse<LegalType>(LegalType, ignoreCase: true, out var legalTypeEnum))
            throw new InvalidOperationException($"Invalid LegalType: {LegalType}. Must be 'Individual' or 'Corporate'.");

        // Parse RiskLevel enum (optional)
        RiskLevel? riskLevelEnum = null;
        if (!string.IsNullOrWhiteSpace(RiskLevel) && Enum.TryParse<RiskLevel>(RiskLevel, ignoreCase: true, out var parsedRisk))
            riskLevelEnum = parsedRisk;

        // Build address JSON
        var address = new
        {
            street = Street,
            city = City,
            state = State,
            zipCode = ZipCode,
            country = Country
        };

        var addressJson = JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(address));

        return new CreateClientRequest(
            LegalType: legalTypeEnum,
            Name: Name,
            ContactNumber: ContactNumber,
            AddressJson: addressJson,
            CountryCode: CountryCode,
            InitialRiskLevel: riskLevelEnum
        );
    }
}
