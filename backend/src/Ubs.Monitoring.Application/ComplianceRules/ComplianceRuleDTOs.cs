using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.ComplianceRules;

/// <summary>
/// Application-level DTO representing a compliance rule.
/// </summary>
/// <param name="Id">
/// The unique identifier of the compliance rule.
/// </param>
/// <param name="Code">
/// The immutable system code that uniquely identifies the compliance rule.
/// </param>
/// <param name="RuleType">
/// The type or category of the compliance rule.
/// </param>
/// <param name="Name">
/// The human-readable name of the compliance rule.
/// </param>
/// <param name="IsActive">
/// Indicates whether the compliance rule is currently active.
/// </param>
/// <param name="Severity">
/// The severity level assigned to the compliance rule.
/// </param>
/// <param name="Scope">
/// The optional scope expression that defines where the rule applies.
/// </param>
/// <param name="Parameters">
/// A JSON object containing rule-specific configuration parameters.
/// </param>
/// <param name="CreatedAtUtc">
/// The UTC timestamp indicating when the compliance rule was created.
/// </param>
/// <param name="UpdatedAtUtc">
/// The UTC timestamp indicating when the compliance rule was last updated.
/// </param>
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

/// <summary>
/// Application-level DTO representing a partial update request for a compliance rule.
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
/// The optional scope expression that limits where the rule applies.
/// </param>
/// <param name="Parameters">
/// A JSON object containing rule-specific configuration parameters.
/// </param>
public record PatchComplianceRuleDto(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);
