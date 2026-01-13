using Ubs.Monitoring.Domain.Enums;
using System.Text.Json;

namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Request for partially updating a compliance rule.
/// </summary>
/// <param name="Name">New display name for the rule.</param>
/// <param name="IsActive">Whether the rule should be active.</param>
/// <param name="Severity">Alert severity level (Low, Medium, High, Critical).</param>
/// <param name="Scope">Rule scope - country code (e.g., 'BR') or 'global'.</param>
/// <param name="Parameters">Rule-specific configuration as JSON.</param>
public sealed record PatchRuleRequest(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);