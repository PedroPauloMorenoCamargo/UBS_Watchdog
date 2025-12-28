using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public AuditAction Action { get; set; }

    public Guid PerformedByAnalystId { get; set; }
    public Analyst PerformedByAnalyst { get; set; } = null!;

    public DateTimeOffset PerformedAtUtc { get; set; }

    public JsonDocument? BeforeJson { get; set; }
    public JsonDocument? AfterJson { get; set; }

    public string? CorrelationId { get; set; }
}
