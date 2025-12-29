using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class AuditLog
{
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
