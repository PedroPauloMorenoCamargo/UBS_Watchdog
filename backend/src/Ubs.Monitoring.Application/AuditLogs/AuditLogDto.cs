using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.AuditLogs;

public sealed record AuditLogDto(
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
