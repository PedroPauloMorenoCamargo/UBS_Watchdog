using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

public sealed record PatchComplianceRuleData(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);
