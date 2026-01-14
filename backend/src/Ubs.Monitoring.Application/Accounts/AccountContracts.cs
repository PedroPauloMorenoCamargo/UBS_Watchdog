using Ubs.Monitoring.Application.AccountIdentifiers;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Accounts;

/// <summary>
/// Request DTO for creating a new account.
/// </summary>
public sealed record CreateAccountRequest(
    string AccountIdentifier,
    string CountryCode,
    AccountType AccountType,
    string CurrencyCode
);

/// <summary>
/// Response DTO with basic account information.
/// </summary>
public sealed record AccountResponseDto(
    Guid Id,
    Guid ClientId,
    string AccountIdentifier,
    string CountryCode,
    AccountType AccountType,
    string CurrencyCode,
    AccountStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

/// <summary>
/// Response DTO with detailed account information including identifiers.
/// </summary>
public sealed record AccountDetailDto(
    Guid Id,
    Guid ClientId,
    string? ClientName,
    string AccountIdentifier,
    string CountryCode,
    AccountType AccountType,
    string CurrencyCode,
    AccountStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<AccountIdentifierDto> Identifiers
);

/// <summary>
/// Response DTO for account import operations.
/// </summary>
public sealed record AccountImportResultDto(
    int TotalProcessed,
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<AccountImportErrorDto> Errors
);

/// <summary>
/// Details of an account import error.
/// </summary>
public sealed record AccountImportErrorDto(
    int LineNumber,
    string AccountIdentifier,
    string ErrorMessage
);
