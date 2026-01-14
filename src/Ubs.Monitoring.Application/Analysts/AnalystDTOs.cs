namespace Ubs.Monitoring.Application.Analysts;

/// <summary>
/// Application-level DTO representing an analyst profile. Used as an output boundary from the Application layer.
/// </summary>
/// <param name="Id">
/// The unique identifier of the analyst.
/// </param>
/// <param name="CorporateEmail">
/// The corporate email address of the analyst.
/// </param>
/// <param name="FullName">
/// The full name of the analyst.
/// </param>
/// <param name="PhoneNumber">
/// The optional phone number of the analyst.
/// </param>
/// <param name="ProfilePictureBase64">
/// A Base64-encoded representation of the analyst’s profile picture, if available.
/// </param>
/// <param name="CreatedAtUtc">
/// The UTC timestamp indicating when the analyst account was created.
/// </param>
public sealed record AnalystProfileDto(
    Guid Id,
    string CorporateEmail,
    string FullName,
    string? PhoneNumber,
    string? ProfilePictureBase64,
    DateTimeOffset CreatedAtUtc
);
