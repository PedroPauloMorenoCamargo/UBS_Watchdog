using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Cases;

#region Request DTOs

/// <summary>
/// Filter and pagination request for listing cases.
/// </summary>
public sealed record CaseFilterRequest(
    Guid? ClientId = null,
    Guid? AccountId = null,
    Guid? TransactionId = null,
    Guid? AnalystId = null,
    CaseStatus? Status = null,
    CaseDecision? Decision = null,
    Severity? Severity = null,
    DateTimeOffset? OpenedFrom = null,
    DateTimeOffset? OpenedTo = null,
    DateTimeOffset? ResolvedFrom = null,
    DateTimeOffset? ResolvedTo = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = true
);

/// <summary>
/// Request to update case workflow (status, decision, analyst).
/// </summary>
public sealed record UpdateCaseRequest(
    CaseStatus? Status = null,
    CaseDecision? Decision = null,
    Guid? AnalystId = null
);

/// <summary>
/// Filter request for listing case findings.
/// </summary>
public sealed record CaseFindingFilterRequest(
    string? SortBy = null,
    bool SortDescending = true
);

#endregion

#region Response DTOs

/// <summary>
/// Summary DTO for case listing.
/// </summary>
public sealed record CaseResponseDto(
    Guid Id,
    Guid TransactionId,
    Guid ClientId,
    string ClientName,
    Guid AccountId,
    string AccountIdentifier,
    CaseStatus Status,
    CaseDecision? Decision,
    Guid? AnalystId,
    string? AnalystName,
    Severity Severity,
    int FindingsCount,
    DateTimeOffset OpenedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ResolvedAtUtc
);

/// <summary>
/// Detailed DTO for viewing a single case.
/// </summary>
public sealed record CaseDetailDto(
    Guid Id,
    Guid TransactionId,
    TransactionSummaryDto Transaction,
    Guid ClientId,
    ClientSummaryDto Client,
    Guid AccountId,
    AccountSummaryDto Account,
    CaseStatus Status,
    CaseDecision? Decision,
    Guid? AnalystId,
    AnalystSummaryDto? Analyst,
    Severity Severity,
    IReadOnlyList<CaseFindingDto> Findings,
    DateTimeOffset OpenedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ResolvedAtUtc
);

/// <summary>
/// Summary DTO for transaction within case detail.
/// </summary>
public sealed record TransactionSummaryDto(
    Guid Id,
    TransactionType Type,
    TransferMethod? TransferMethod,
    decimal Amount,
    string CurrencyCode,
    decimal BaseAmount,
    string BaseCurrencyCode,
    DateTimeOffset OccurredAtUtc
);

/// <summary>
/// Summary DTO for client within case detail.
/// </summary>
public sealed record ClientSummaryDto(
    Guid Id,
    string Name,
    LegalType LegalType,
    string CountryCode,
    RiskLevel RiskLevel,
    KycStatus KycStatus
);

/// <summary>
/// Summary DTO for account within case detail.
/// </summary>
public sealed record AccountSummaryDto(
    Guid Id,
    string AccountIdentifier,
    AccountType AccountType,
    string CountryCode,
    string CurrencyCode,
    AccountStatus Status
);

/// <summary>
/// Summary DTO for analyst within case detail.
/// </summary>
public sealed record AnalystSummaryDto(
    Guid Id,
    string FullName,
    string CorporateEmail
);

/// <summary>
/// DTO for case finding.
/// </summary>
public sealed record CaseFindingDto(
    Guid Id,
    Guid CaseId,
    Guid RuleId,
    string RuleName,
    string RuleCode,
    RuleType RuleType,
    Severity Severity,
    JsonDocument EvidenceJson,
    DateTimeOffset CreatedAtUtc
);

#endregion
