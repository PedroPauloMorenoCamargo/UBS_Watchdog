using Ubs.Monitoring.Application.Analysts;
namespace Ubs.Monitoring.Application.Auth;

public sealed record LoginResultDto(
    string Token,
    DateTime ExpiresAtUtc,
    AnalystProfileDto Analyst
);


public sealed record AnalystAuthDto(
    Guid Id,
    string CorporateEmail,
    string FullName,
    string PasswordHash,
    string? PhoneNumber,
    string? ProfilePictureBase64,
    DateTimeOffset CreatedAtUtc
)
{
    public AnalystProfileDto ToProfileDto() =>
        new(
            Id,
            CorporateEmail,
            FullName,
            PhoneNumber,
            ProfilePictureBase64,
            CreatedAtUtc
        );
}