using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.AccountIdentifiers;

/// <summary>
/// DTO for account identifier information.
/// </summary>
public sealed record AccountIdentifierDto(
    Guid Id,
    IdentifierType IdentifierType,
    string IdentifierValue,
    string? IssuedCountryCode,
    DateTimeOffset CreatedAtUtc
);

/// <summary>
/// Request DTO for creating a new account identifier.
/// </summary>
public sealed record CreateAccountIdentifierRequest(
    IdentifierType IdentifierType,
    string IdentifierValue,
    string? IssuedCountryCode = null
);
