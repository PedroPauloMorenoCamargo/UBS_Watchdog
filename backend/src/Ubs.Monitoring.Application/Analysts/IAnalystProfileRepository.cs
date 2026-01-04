using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Analysts;

public interface IAnalystProfileRepository
{
    Task<Analyst?> GetForUpdateAsync(Guid analystId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<Analyst?> GetByIdAsync(Guid id, CancellationToken ct);
}
