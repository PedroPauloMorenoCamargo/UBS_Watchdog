using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Analysts;

public interface IAnalystProfileRepository
{
    // Command-side
    Task<Analyst?> GetForUpdateAsync(Guid analystId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    // Query-side
    Task<AnalystProfileDto?> GetProfileByIdAsync(Guid analystId, CancellationToken ct);
}
