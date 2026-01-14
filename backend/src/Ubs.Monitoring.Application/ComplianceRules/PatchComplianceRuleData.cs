using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;
/// <summary>
/// Represents a data structure used to apply partial updates to a compliance rule.
/// </summary>
/// <param name="Name">
/// The new display name of the compliance rule.
/// </param>
/// <param name="IsActive">
/// Indicates whether the compliance rule should be enabled or disabled.
/// </param>
/// <param name="Severity">
/// The severity level associated with the compliance rule.
/// </param>
/// <param name="Scope">
/// The optional scope expression that defines where the rule applies.
/// </param>
/// <param name="Parameters">
/// A JSON obj
public sealed record PatchComplianceRuleData(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);
