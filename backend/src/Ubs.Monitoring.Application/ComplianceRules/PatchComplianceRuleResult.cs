namespace Ubs.Monitoring.Application.ComplianceRules;

public enum PatchComplianceRuleStatus
{
    Success,
    NotFound,
    NoChanges,
    InvalidParameters,
    InvalidUpdate
}

public sealed record PatchComplianceRuleResult(
    PatchComplianceRuleStatus Status,
    ComplianceRuleDto? Rule = null,
    IReadOnlyList<string>? Errors = null
);
