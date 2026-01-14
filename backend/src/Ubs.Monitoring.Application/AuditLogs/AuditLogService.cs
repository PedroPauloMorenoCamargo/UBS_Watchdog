using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.AuditLogs;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repo;

    public AuditLogService(IAuditLogRepository repo) => _repo = repo;

    public async Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken ct)
    {
        var result = await _repo.SearchAsync(query, ct);
        return result.Map(AuditLogMapper.ToDto);
    }

    public async Task<AuditLogDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var x = await _repo.GetByIdAsync(id, ct);
        return x is null ? null : AuditLogMapper.ToDto(x);
    }
}
