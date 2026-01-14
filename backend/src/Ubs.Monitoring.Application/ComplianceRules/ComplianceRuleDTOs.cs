using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

public sealed record ComplianceRuleDto(
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


public record PatchComplianceRuleDto(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);