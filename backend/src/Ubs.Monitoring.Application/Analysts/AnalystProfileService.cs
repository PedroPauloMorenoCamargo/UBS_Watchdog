namespace Ubs.Monitoring.Application.Analysts;

public sealed class AnalystProfileService : IAnalystProfileService
{
    private const int MaxProfilePictureBytes = 2 * 1024 * 1024; // 2MB
    private readonly IAnalystProfileRepository _repo;

    public AnalystProfileService(IAnalystProfileRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Updates or clears the profile picture of an analyst.
    /// </summary>
    /// <param name="analystId">
    /// Identifier of the analyst whose profile picture should be updated.
    /// </param>
    /// <param name="base64">
    /// Base64-encoded image data. May optionally include a data URI prefix
    /// (e.g. <c>data:image/png;base64,...</c>).  
    /// If <c>null</c>, empty, or whitespace, the profile picture will be cleared.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// <c>true</c> if the update (or clear) succeeded;  
    /// <c>false</c> if the analyst does not exist.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="base64"/> is not valid Base64 or exceeds the maximum allowed size.
    /// </exception>
    public async Task<bool> UpdateProfilePictureAsync(Guid analystId, string? base64, CancellationToken ct)
    {
        var analyst = await _repo.GetForUpdateAsync(analystId, ct);
        if (analyst is null) return false;

        // allow clearing
        if (string.IsNullOrWhiteSpace(base64))
        {
            analyst.UpdateProfilePicture(null);
            await _repo.SaveChangesAsync(ct);
            return true;
        }

        // accept optional data-uri prefix
        base64 = base64.Trim();
        var commaIdx = base64.IndexOf(',');
        if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIdx > 0)
            base64 = base64[(commaIdx + 1)..];

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch
        {
            throw new ArgumentException("profilePictureBase64 is not valid base64.");
        }

        if (bytes.Length > MaxProfilePictureBytes)
            throw new ArgumentException("profilePictureBase64 exceeds 2MB limit.");

        // store normalized base64 (no data-uri)
        analyst.UpdateProfilePicture(Convert.ToBase64String(bytes));
        await _repo.SaveChangesAsync(ct);
        return true;
    }
}
