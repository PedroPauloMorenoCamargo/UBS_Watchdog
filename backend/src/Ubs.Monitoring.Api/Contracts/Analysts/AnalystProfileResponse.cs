namespace Ubs.Monitoring.Api.Contracts;
public sealed record AnalystProfileResponse(
    Guid Id,
    string CorporateEmail,
    string FullName,
    string? PhoneNumber,
    string? ProfilePictureBase64,
    DateTimeOffset CreatedAtUtc
);
