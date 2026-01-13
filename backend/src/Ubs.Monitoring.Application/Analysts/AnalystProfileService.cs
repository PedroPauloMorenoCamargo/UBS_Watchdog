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
    /// Updates or clears profile picture. Accepts optional data URI prefix.
    /// Max size: 2MB. If null/empty, clears the picture.
    /// </summary>
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
