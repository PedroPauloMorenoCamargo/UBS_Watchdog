using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Application.Analysts;
namespace Ubs.Monitoring.Api.Mappers;

public static class AuthContractMapper
{
    public static LoginResponse ToLoginResponse(LoginResultDto src)
        => new(
            Token: src.Token,
            ExpiresAtUtc: src.ExpiresAtUtc,
            Analyst: ToAnalystProfileResponse(src.Analyst)
        );

    public static AnalystProfileResponse ToAnalystProfileResponse(AnalystProfileDto src)
        => new(
            Id: src.Id,
            CorporateEmail: src.CorporateEmail,
            FullName: src.FullName,
            PhoneNumber: src.PhoneNumber,
            ProfilePictureBase64: src.ProfilePictureBase64,
            CreatedAtUtc: src.CreatedAtUtc
        );
}
