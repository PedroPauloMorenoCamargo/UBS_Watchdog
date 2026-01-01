using System.Security.Claims;
using Ubs.Monitoring.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;

namespace Ubs.Monitoring.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAnalystRepository _analysts;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    public AuthService( IAnalystRepository analysts, IPasswordHasher hasher, ITokenService tokens)
    {
        _analysts = analysts;
        _hasher = hasher;
        _tokens = tokens;
    }

    /// <summary>
    /// Authenticates an analyst using email and password credentials.
    /// </summary>
    /// <param name="email">
    /// The analyst’s corporate email address.
    /// </param>
    /// <param name="password">
    /// The plaintext password provided by the analyst.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="LoginResultDto"/> containing the authentication token and
    /// analyst profile if authentication succeeds; otherwise, <c>null</c>.
    /// </returns>
    public async Task<LoginResultDto?> LoginAsync( string email, string password, CancellationToken ct)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
            return null;

        var analyst = await _analysts.GetByEmailAsync(normalizedEmail, ct);
        if (analyst is null)
            return null;

        if (!_hasher.Verify(password, analyst.PasswordHash))
            return null;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, analyst.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, analyst.CorporateEmail),
            new Claim(ClaimTypes.Name, analyst.FullName),
            new Claim(ClaimTypes.Role, "Analyst")
        };

        var (token, expiresAtUtc) = _tokens.CreateToken(claims);

        return new LoginResultDto(
            Token: token,
            ExpiresAtUtc: expiresAtUtc,
            Analyst: MapProfile(analyst)
        );
    }

    /// <summary>
    /// Retrieves the profile of the authenticated analyst.
    /// </summary>
    /// <param name="analystId">
    /// The unique identifier of the analyst.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="AnalystProfileDto"/> if the analyst exists; otherwise,
    /// <c>null</c>.
    /// </returns>
    public async Task<AnalystProfileDto?> GetMeAsync(Guid analystId, CancellationToken ct)
    {
        var analyst = await _analysts.GetByIdAsync(analystId, ct);
        return analyst is null ? null : MapProfile(analyst);
    }

    /// <summary>
    /// Maps a domain <see cref="Analyst"/> entity to an
    /// <see cref="AnalystProfileDto"/>.
    /// </summary>
    /// <param name="a">
    /// The analyst domain entity.
    /// </param>
    /// <returns>
    /// A data transfer object representing the analyst profile.
    /// </returns>
    private static AnalystProfileDto MapProfile(Analyst a) =>
        new(
            a.Id,
            a.CorporateEmail,
            a.FullName,
            a.PhoneNumber,
            a.ProfilePictureBase64,
            a.CreatedAtUtc
        );
}
