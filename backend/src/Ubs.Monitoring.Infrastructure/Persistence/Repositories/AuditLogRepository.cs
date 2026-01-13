using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.AuditLogs;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;
using Ubs.Monitoring.Infrastructure.Persistence.Pagination;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<AuditLog>> SearchAsync(AuditLogQuery q, CancellationToken ct)
    {
        IQueryable<AuditLog> query = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.EntityType))
            query = query.Where(x => x.EntityType == q.EntityType);

        if (!string.IsNullOrWhiteSpace(q.EntityId))
            query = query.Where(x => x.EntityId == q.EntityId);

        if (q.Action is not null)
            query = query.Where(x => x.Action == q.Action);

        if (q.PerformedByAnalystId is not null)
            query = query.Where(x => x.PerformedByAnalystId == q.PerformedByAnalystId);

        if (!string.IsNullOrWhiteSpace(q.CorrelationId))
            query = query.Where(x => x.CorrelationId != null &&
                                     EF.Functions.Like(x.CorrelationId, $"%{q.CorrelationId}%"));

        if (q.FromUtc is not null)
            query = query.Where(x => x.PerformedAtUtc >= q.FromUtc.Value);

        if (q.ToUtc is not null)
            query = query.Where(x => x.PerformedAtUtc <= q.ToUtc.Value);

        query = ApplySort(query, q.Page.SortBy, q.Page.SortDir);

        return await query.ToPagedResultAsync(q.Page, ct);
    }

    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    private static IQueryable<AuditLog> ApplySort(IQueryable<AuditLog> query, string? sortBy, string? sortDir)
    {
        var sb = (sortBy ?? "PerformedAtUtc").Trim();
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        // Stable ordering: sempre desempata por Id
        return sb.ToLowerInvariant() switch
        {
            "entitytype" => (desc ? query.OrderByDescending(x => x.EntityType) : query.OrderBy(x => x.EntityType)).ThenBy(x => x.Id),
            "entityid" => (desc ? query.OrderByDescending(x => x.EntityId) : query.OrderBy(x => x.EntityId)).ThenBy(x => x.Id),
            "action" => (desc ? query.OrderByDescending(x => x.Action) : query.OrderBy(x => x.Action)).ThenBy(x => x.Id),
            "performedbyanalystid" => (desc ? query.OrderByDescending(x => x.PerformedByAnalystId) : query.OrderBy(x => x.PerformedByAnalystId)).ThenBy(x => x.Id),
            "correlationid" => (desc ? query.OrderByDescending(x => x.CorrelationId) : query.OrderBy(x => x.CorrelationId)).ThenBy(x => x.Id),
            "performedatutc" or _ => (desc ? query.OrderByDescending(x => x.PerformedAtUtc) : query.OrderBy(x => x.PerformedAtUtc)).ThenBy(x => x.Id),
        };
    }
}
