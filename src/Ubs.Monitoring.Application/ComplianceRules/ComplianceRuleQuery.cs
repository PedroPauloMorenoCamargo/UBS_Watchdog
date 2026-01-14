using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

public sealed class ComplianceRuleQuery
{
    public PageRequest Page { get; init; } = new();

    public RuleType? RuleType { get; init; }
    public bool? IsActive { get; init; }
    public Severity? Severity { get; init; }
    public string? Scope { get; init; }
}
