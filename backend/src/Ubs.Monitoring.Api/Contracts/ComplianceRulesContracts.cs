using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Represents a partial update request for an existing compliance rule. Only the provided fields will be updated.
/// </summary>
/// <param name="Name">
/// The new display name of the compliance rule.
/// </param>
/// <param name="IsActive">
/// Indicates whether the rule should be enabled or disabled.
/// </param>
/// <param name="Severity">
/// The severity level associated with the rule.
/// </param>
/// <param name="Scope">
/// The optional scope expression that limits where the rule applies.
/// </param>
/// <param name="Parameters">
/// A JSON object containing rule-specific configuration parameters.
/// </param>
public sealed record PatchComplianceRuleRequest(
    string? Name,
    bool? IsActive,
    Severity? Severity,
    string? Scope,
    JsonElement? Parameters
);

/// <summary>
/// Represents a compliance rule returned by the API.
/// </summary>
/// <param name="Id">
/// The unique identifier of the compliance rule.
/// </param>
/// <param name="Code">
/// The immutable system code that uniquely identifies the rule.
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
/// The severity level assigned to the rule.
/// </param>
/// <param name="Scope">
/// The optional scope expression that defines where the rule applies.
/// </param>
/// <param name="Parameters">
/// A JSON object containing the rule’s configuration parameters.
/// </param>
/// <param name="CreatedAtUtc">
/// The UTC timestamp indicating when the rule was created.
/// </param>
/// <param name="UpdatedAtUtc">
/// The UTC timestamp indicating when the rule was last updated.
/// </param>
public sealed record ComplianceRuleResponse(
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
/// Represents a request to search and filter compliance rules with pagination and optional criteria.
/// </summary>
/// <param name="Page">
/// The 1-based page number to retrieve.
/// </param>
/// <param name="PageSize">
/// The maximum number of records to return per page.
/// </param>
/// <param name="SortBy">
/// The name of the field used to sort the results.
/// </param>
/// <param name="SortDir">
/// The sort direction, typically <c>asc</c> or <c>desc</c>.
/// </param>
/// <param name="RuleType">
/// Optional filter for the compliance rule type.
/// </param>
/// <param name="IsActive">
/// Optional filter indicating whether the rule is active.
/// </param>
/// <param name="Severity">
/// Optional filter for the severity level.
/// </param>
/// <param name="Scope">
/// Optional filter for the rule scope.
/// </param>
public sealed record SearchComplianceRulesRequest(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = "desc",
    RuleType? RuleType = null,
    bool? IsActive = null,
    Severity? Severity = null,
    string? Scope = null
);
