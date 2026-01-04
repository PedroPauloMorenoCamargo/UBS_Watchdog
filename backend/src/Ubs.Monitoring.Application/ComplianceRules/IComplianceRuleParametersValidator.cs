using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

public interface IComplianceRuleParametersValidator
{
    IReadOnlyList<string> Validate(RuleType ruleType, JsonElement parameters);
}
