using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Contracts.AuditLogs;

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
