using System.Text.Json;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.AuditLogs;

public static class AuditLogMapper
{
    public static AuditLogDto ToDto(AuditLog x)
    {
        if (x is null)
            throw new ArgumentNullException(nameof(x));

        JsonElement? before = CloneOrNull(x.BeforeJson);
        JsonElement? after  = CloneOrNull(x.AfterJson);

        return new AuditLogDto(
            x.Id,
            x.EntityType,
            x.EntityId,
            x.Action,
            x.PerformedByAnalystId,
            x.CorrelationId,
            before,
            after,
            x.PerformedAtUtc
        );
    }

    private static JsonElement? CloneOrNull(JsonDocument? doc)
    {
        return doc is null ? null : doc.RootElement.Clone();
    }
}
