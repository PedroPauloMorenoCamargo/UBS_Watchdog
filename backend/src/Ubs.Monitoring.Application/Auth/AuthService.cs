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

    public async Task<AnalystProfileDto?> GetMeAsync(Guid analystId, CancellationToken ct)
    {
        var analyst = await _analysts.GetByIdAsync(analystId, ct);
        return analyst is null ? null : MapProfile(analyst);
    }

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
