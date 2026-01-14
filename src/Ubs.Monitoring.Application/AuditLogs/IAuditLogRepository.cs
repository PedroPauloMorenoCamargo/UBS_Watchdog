using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.AuditLogs;

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLog>> SearchAsync(AuditLogQuery query, CancellationToken ct);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct);
}
