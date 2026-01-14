namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Request payload to update an analyst profile picture.
/// </summary>
/// <param name="ProfilePictureBase64">
/// Base64-encoded image (optionally prefixed with a data URI, e.g.
/// <c>data:image/png;base64,...</c>).
/// Send <c>null</c> to clear the profile picture.
/// </param>
public sealed record UpdateProfilePictureRequest(string? ProfilePictureBase64);

/// <summary>
/// Response containing analyst profile information.
/// </summary>
/// <param name="Id">Unique identifier of the analyst.</param>
/// <param name="CorporateEmail">The analyst's corporate email address.</param>
/// <param name="FullName">Full name of the analyst.</param>
/// <param name="PhoneNumber">Optional phone number.</param>
/// <param name="ProfilePictureBase64">Optional base64-encoded profile picture.</param>
/// <param name="CreatedAtUtc">Account creation timestamp.</param>
public sealed record AnalystProfileResponse(
    Guid Id,
    string CorporateEmail,
    string FullName,
    string? PhoneNumber,
    string? ProfilePictureBase64,
    DateTimeOffset CreatedAtUtc
);
