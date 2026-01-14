using Ubs.Monitoring.Application.Analysts;

namespace Ubs.Monitoring.Application.Auth;

public interface IAnalystRepository
{
    Task<AnalystAuthDto?> GetAuthByEmailAsync(string normalizedEmail, CancellationToken ct);

    Task<AnalystProfileDto?> GetProfileByIdAsync(Guid analystId, CancellationToken ct);
}
