namespace Ubs.Monitoring.Application.Analysts;

public sealed class AnalystProfileService : IAnalystProfileService
{
    private readonly IAnalystProfileRepository _repo;

    public AnalystProfileService(IAnalystProfileRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Updates or clears the analyst's profile picture.
    /// </summary>
    /// <param name="analystId">
    /// Unique identifier of the analyst whose profile picture will be updated.
    /// </param>
    /// <param name="profilePictureBase64">
    /// Base64-encoded image string representing the new profile picture.
    /// May optionally include a data URI prefix (e.g. <c>data:image/png;base64,...</c>).
    /// Pass <c>null</c> to remove the existing profile picture.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <c>true</c> if the analyst exists and the profile picture was successfully updated or cleared;
    /// <c>false</c> if the analyst does not exist.
    /// </returns>
    public async Task<bool> UpdateProfilePictureAsync( Guid analystId, string? profilePictureBase64, CancellationToken ct)
    {
        var analyst = await _repo.GetForUpdateAsync(analystId, ct);
        if (analyst is null)
            return false;
        if (profilePictureBase64 is null)
        {
            analyst.UpdateProfilePicture(null);
            await _repo.SaveChangesAsync(ct);
            return true;
        }
        analyst.UpdateProfilePicture(profilePictureBase64);

        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
