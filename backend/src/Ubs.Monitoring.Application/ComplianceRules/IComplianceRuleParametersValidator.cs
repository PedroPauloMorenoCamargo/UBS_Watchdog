using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

/// <summary>
/// Validates compliance rule parameters for different rule types.
/// Performs both structural validation (format, types) and data validation (database checks).
/// </summary>
public interface IComplianceRuleParametersValidator
{
    /// <summary>
    /// Performs complete validation including structural checks and data existence validation.
    /// This is the recommended method for all validation scenarios.
    /// </summary>
    /// <param name="ruleType">The type of compliance rule being validated.</param>
    /// <param name="parameters">JSON parameters to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of validation errors (empty if valid).</returns>
    Task<IReadOnlyList<string>> ValidateAsync(RuleType ruleType, JsonElement parameters, CancellationToken ct = default);
}
