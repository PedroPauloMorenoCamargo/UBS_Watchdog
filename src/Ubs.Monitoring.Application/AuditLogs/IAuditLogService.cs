using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.AuditLogs;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken ct);
    Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken ct);
}
