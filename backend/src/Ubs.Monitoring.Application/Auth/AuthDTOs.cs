using Ubs.Monitoring.Application.Analysts;
namespace Ubs.Monitoring.Application.Auth;

// <summary>
/// Represents the result of a successful authentication operation.
/// </summary>
/// <param name="Token">
/// The JWT access token issued to the authenticated analyst.
/// </param>
/// <param name="ExpiresAtUtc">
/// The UTC timestamp indicating when the token expires.
/// </param>
/// <param name="Analyst">
/// The authenticated analyst profile information.
/// </param>
public sealed record LoginResultDto(
    string Token,
    DateTime ExpiresAtUtc,
    AnalystProfileDto Analyst
);

/// <summary>
/// Represents authentication-related data for an analyst.
/// </summary>
/// <param name="Id">
/// The unique identifier of the analyst.
/// </param>
/// <param name="CorporateEmail">
/// The corporate email address used for authentication.
/// </param>
/// <param name="FullName">
/// The full name of the analyst.
/// </param>
/// <param name="PasswordHash">
/// The hashed password associated with the analyst account.
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
    /// <summary>
    /// Converts the authentication DTO into a public analyst profile DTO.
    /// </summary>
    /// <returns>
    /// An <see cref="AnalystProfileDto"/> containing non-sensitive analyst information.
    /// </returns>
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