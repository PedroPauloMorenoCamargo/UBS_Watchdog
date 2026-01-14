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

    public async Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken ct)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
            return null;

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

    public Task<AnalystProfileDto?> GetMeAsync(Guid analystId, CancellationToken ct)
    {
        return _analysts.GetProfileByIdAsync(analystId, ct);
    }
}
