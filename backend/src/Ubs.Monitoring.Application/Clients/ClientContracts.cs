using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Request DTO for creating a new client.
/// </summary>
public sealed record CreateClientRequest(
    LegalType LegalType,
    string Name,
    string ContactNumber,
    JsonDocument AddressJson,
    string CountryCode,
    RiskLevel? InitialRiskLevel = null
);

/// <summary>
/// Request DTO for updating client contact information.
/// </summary>
public sealed record UpdateClientContactRequest(
    string ContactNumber,
    JsonDocument? AddressJson = null
);

/// <summary>
/// Request DTO for updating client KYC status.
/// </summary>
public sealed record UpdateClientKycRequest(
    KycStatus NewStatus
);

/// <summary>
/// Request DTO for updating client risk level.
/// </summary>
public sealed record UpdateClientRiskRequest(
    RiskLevel NewRiskLevel
);

/// <summary>
/// Response DTO with basic client information.
/// </summary>
public sealed record ClientResponseDto(
    Guid Id,
    LegalType LegalType,
    string Name,
    string ContactNumber,
    JsonDocument AddressJson,
    string CountryCode,
    RiskLevel RiskLevel,
    KycStatus KycStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

/// <summary>
/// Response DTO with detailed client information including related entities.
/// </summary>
public sealed record ClientDetailDto(
    Guid Id,
    LegalType LegalType,
    string Name,
    string ContactNumber,
    JsonDocument AddressJson,
    string CountryCode,
    RiskLevel RiskLevel,
    KycStatus KycStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int TotalAccounts,
    int TotalTransactions,
    int TotalCases
);

/// <summary>
/// Response DTO for paginated list of clients.
/// </summary>
public sealed record PagedClientsResponseDto(
    IReadOnlyList<ClientResponseDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
