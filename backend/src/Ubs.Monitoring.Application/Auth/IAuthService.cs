namespace Ubs.Monitoring.Application.Auth;

public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken ct);
    Task<AnalystProfileDto?> GetMeAsync(Guid analystId, CancellationToken ct);
}
