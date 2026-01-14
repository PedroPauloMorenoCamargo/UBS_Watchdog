using Ubs.Monitoring.Domain.Enums;
using System.Text.Json;

namespace Ubs.Monitoring.Api.Contracts;

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
