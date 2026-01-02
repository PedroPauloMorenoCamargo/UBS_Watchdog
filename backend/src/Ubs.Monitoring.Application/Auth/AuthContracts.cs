namespace Ubs.Monitoring.Application.Auth;

public sealed record AnalystProfileDto(
    Guid Id,
    string CorporateEmail,
    string FullName,
    string? PhoneNumber,
    string? ProfilePictureBase64,
    DateTimeOffset CreatedAtUtc
);


public sealed record LoginResultDto(
    string Token,
    DateTime ExpiresAtUtc,
    AnalystProfileDto Analyst
);
