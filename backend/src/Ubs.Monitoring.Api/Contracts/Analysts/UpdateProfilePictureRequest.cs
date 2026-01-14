namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Request for updating an analyst's profile picture.
/// </summary>
/// <param name="ProfilePictureBase64">Base64-encoded image (JPEG/PNG, max 1MB) or null to clear.</param>
public sealed record UpdateProfilePictureRequest(
    string? ProfilePictureBase64
);
