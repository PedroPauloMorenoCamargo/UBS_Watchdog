namespace Ubs.Monitoring.Application.Analysts;

/// <summary>
/// Application-level DTO representing an analyst profile.
/// Used as an output boundary from the Application layer.
/// </summary>
public sealed record AnalystProfileDto(
    Guid Id,
    string CorporateEmail,
    string FullName,
    string? PhoneNumber,
    string? ProfilePictureBase64,
    DateTimeOffset CreatedAtUtc
);
