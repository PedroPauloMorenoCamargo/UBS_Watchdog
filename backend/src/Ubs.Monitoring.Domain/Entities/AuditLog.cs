using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class AuditLog
{
    private AuditLog() { }

    public AuditLog(
        string entityType,
        string entityId,
        AuditAction action,
        Guid performedByAnalystId,
        JsonDocument? beforeJson = null,
        JsonDocument? afterJson = null,
        string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID is required", nameof(entityId));

        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        PerformedByAnalystId = performedByAnalystId;
        PerformedAtUtc = DateTimeOffset.UtcNow;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        CorrelationId = correlationId;
    }

    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public AuditAction Action { get; private set; }

    public Guid PerformedByAnalystId { get; private set; }
    public Analyst PerformedByAnalyst { get; set; } = null!; 

    public DateTimeOffset PerformedAtUtc { get; private set; }

    public JsonDocument? BeforeJson { get; private set; }
    public JsonDocument? AfterJson { get; private set; }

    public string? CorrelationId { get; private set; }
}
