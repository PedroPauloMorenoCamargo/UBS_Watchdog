using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Application.Analysts;

namespace Ubs.Monitoring.Api.Mappers;

/// <summary>
/// Maps analyst-related Application DTOs to API response contracts.
/// </summary>
public static class AnalystProfileMapper
{
    public static AnalystProfileResponse ToResponse(AnalystProfileDto dto)
        => new(
            Id: dto.Id,
            CorporateEmail: dto.CorporateEmail,
            FullName: dto.FullName,
            PhoneNumber: dto.PhoneNumber,
            ProfilePictureBase64: dto.ProfilePictureBase64,
            CreatedAtUtc: dto.CreatedAtUtc
        );
}
