using System.Text.Json;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.AuditLogs;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repo;

    public AuditLogService(IAuditLogRepository repo) => _repo = repo;

    public async Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken ct)
    {
        var result = await _repo.SearchAsync(query, ct);
        return result.Map(ToDto);
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var log = await _repo.GetByIdAsync(id, ct);
        return log is null ? null : ToDto(log);
    }

    private static AuditLogDto ToDto(AuditLog x)
    {
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
        // Importante: JsonDocument é IDisposable. Clone evita “use-after-dispose”.
        return doc is null ? null : doc.RootElement.Clone();
    }
}
