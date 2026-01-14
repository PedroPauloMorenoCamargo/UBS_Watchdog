using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ubs.Monitoring.Application.Analysts;

namespace Ubs.Monitoring.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAnalystRepository _analysts;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IAnalystRepository analysts, IPasswordHasher hasher, ITokenService tokens)
    {
        _analysts = analysts;
        _hasher = hasher;
        _tokens = tokens;
    }
    /// <summary>
    /// Authenticates an analyst using email and password credentials.
    /// </summary>
    /// <param name="email">
    /// The analyst’s email address.
    /// </param>
    /// <param name="password">
    /// The analyst’s plaintext password.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A <see cref="LoginResultDto"/> containing a JWT token, expiration metadata, and the analyst profile when authentication succeeds; otherwise,
    /// <c>null</c> 
    public async Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        // DTO-returning repository (no Domain entity here)
        var analyst = await _analysts.GetAuthByEmailAsync(normalizedEmail, ct);
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
            Analyst: analyst.ToProfileDto()
        );
    }
    /// <summary>
    /// Retrieves the profile of the authenticated analyst.
    /// </summary>
    /// <param name="analystId">
    /// The unique identifier of the analyst.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The analyst profile if found; otherwise, <c>null</c>.
    public Task<AnalystProfileDto?> GetMeAsync(Guid analystId, CancellationToken ct)
    {
        return _analysts.GetProfileByIdAsync(analystId, ct);
    }
}
