using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Contracts;

public sealed record PatchComplianceRuleRequest(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);

public sealed record ComplianceRuleResponse(
    Guid Id,
    string Code,
    RuleType RuleType,
    string Name,
    bool IsActive,
    Severity Severity,
    string? Scope,
    JsonElement Parameters,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);

public sealed record SearchComplianceRulesRequest(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = "desc",
    RuleType? RuleType = null,
    bool? IsActive = null,
    Severity? Severity = null,
    string? Scope = null
);
