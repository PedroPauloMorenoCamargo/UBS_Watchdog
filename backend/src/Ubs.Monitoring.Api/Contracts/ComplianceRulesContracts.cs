using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Application.Common.Pagination;

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
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    string? SortDir = "desc",
    RuleType? RuleType = null,
    bool? IsActive = null,
    Severity? Severity = null,
    string? Scope = null
);
