using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.AuditLogs;

public sealed class AuditLogQuery
{
    public PageRequest Page { get; init; } = new();

    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public AuditAction? Action { get; init; }
    public Guid? PerformedByAnalystId { get; init; }
    public string? CorrelationId { get; init; }

    public DateTimeOffset? FromUtc { get; init; }
    public DateTimeOffset? ToUtc { get; init; }
}
