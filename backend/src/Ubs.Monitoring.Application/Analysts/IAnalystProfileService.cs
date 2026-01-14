namespace Ubs.Monitoring.Application.Analysts;

public interface IAnalystProfileService
{
    Task<bool> UpdateProfilePictureAsync(Guid analystId, string? base64, CancellationToken ct);
}
