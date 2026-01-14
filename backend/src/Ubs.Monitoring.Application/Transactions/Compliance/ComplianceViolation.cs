using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Transactions.Compliance;

public sealed record ComplianceViolation(
    Guid RuleId,
    string RuleCode,
    RuleType RuleType,
    Severity Severity,
    string Message
);
