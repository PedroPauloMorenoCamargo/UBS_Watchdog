using Ubs.Monitoring.Domain.Enums;
using System.Text.Json;

namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Represents a request to search and filter audit log entries with pagination, sorting, and optional filtering criteria.
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
/// <param name="EntityType">
/// Optional filter for the audited entity type.
/// </param>
/// <param name="EntityId">
/// Optional filter for the identifier of the audited entity.
/// </param>
/// <param name="Action">
/// Optional filter for the audit action performed.
/// </param>
/// <param name="PerformedByAnalystId">
/// Optional filter for the identifier of the analyst who performed the action.
/// </param>
/// <param name="CorrelationId">
/// Optional filter for the correlation identifier associated with the request.
/// </param>
/// <param name="FromUtc">
/// Optional lower bound (inclusive) for the audit timestamp in UTC.
/// </param>
/// <param name="ToUtc">
/// Optional upper bound (inclusive) for the audit timestamp in UTC.
/// </param>
public sealed record SearchAuditLogsRequest(
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "PerformedAtUtc",
    string? SortDir = "desc",
    string? EntityType = null,
    string? EntityId = null,
    AuditAction? Action = null,
    Guid? PerformedByAnalystId = null,
    string? CorrelationId = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null
);

/// <summary>
/// Represents a single audit log entry returned by the API.
/// </summary>
/// <param name="Id">
/// The unique identifier of the audit log entry.
/// </param>
/// <param name="EntityType">
/// The type of the entity affected by the audited action.
/// </param>
/// <param name="EntityId">
/// The identifier of the entity affected by the audited action.
/// </param>
/// <param name="Action">
/// The action that was performed on the entity.
/// </param>
/// <param name="PerformedByAnalystId">
/// The identifier of the analyst who performed the action, if available.
/// </param>
/// <param name="CorrelationId">
/// The correlation identifier associated with the originating request.
/// </param>
/// <param name="Before">
/// A JSON snapshot of the entity state before the action was performed.
/// </param>
/// <param name="After">
/// A JSON snapshot of the entity state after the action was performed.
/// </param>
/// <param name="PerformedAtUtc">
/// The UTC timestamp indicating when the action was performed.
/// </param>
public sealed record AuditLogResponse(
    Guid Id,
    string EntityType,
    string EntityId,
    AuditAction Action,
    Guid? PerformedByAnalystId,
    string? CorrelationId,
    JsonElement? Before,
    JsonElement? After,
    DateTimeOffset PerformedAtUtc
);

