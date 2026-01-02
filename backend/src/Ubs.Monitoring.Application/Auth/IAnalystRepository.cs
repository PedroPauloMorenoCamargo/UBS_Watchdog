using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Auth;

public interface IAnalystRepository
{
    Task<Analyst?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Analyst?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
}
