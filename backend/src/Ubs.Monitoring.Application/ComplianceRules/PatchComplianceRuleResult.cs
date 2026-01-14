namespace Ubs.Monitoring.Application.ComplianceRules;

public enum PatchComplianceRuleStatus
{
    Success,
    NotFound,
    NoChanges,
    InvalidParameters,
    InvalidUpdate
}

/// <summary>
/// Represents the result of a compliance rule patch operation.
/// </summary>
/// <param name="Status">
/// The outcome status of the patch operation.
/// </param>
/// <param name="Rule">
/// The updated compliance rule when the operation succeeds; otherwise, <c>null</c>.
/// </param>
/// <param name="Errors">
/// A collecti
public sealed record PatchComplianceRuleResult(
    PatchComplianceRuleStatus Status,
    ComplianceRuleDto? Rule = null,
    IReadOnlyList<string>? Errors = null
);
