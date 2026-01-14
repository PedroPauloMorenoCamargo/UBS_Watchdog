using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Transactions.Compliance;

/// <summary>
/// Represents a compliance violation detected during rule evaluation.
/// </summary>
/// <param name="RuleId">
/// The unique identifier of the compliance rule that was violated.
/// </param>
/// <param name="RuleCode">
/// The system code identifying the violated compliance rule.
/// </param>
/// <param name="RuleType">
/// The type or category of the compliance rule that was violated.
/// </param>
/// <param name="Severity">
/// The severity level associated with the violation.
/// </param>
/// <param name="Message">
/// A human-readable description of the compliance violation.
/// </param>
public sealed record ComplianceViolation(
    Guid RuleId,
    string RuleCode,
    RuleType RuleType,
    Severity Severity,
    string Message
);
