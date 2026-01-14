using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ubs.Monitoring.Application.Auth;

namespace Ubs.Monitoring.Infrastructure.Auth;
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }
    /// <summary>
    /// Creates a signed JSON Web Token (JWT) containing the specified claims.
    /// </summary>
    /// <param name="claims">
    /// Claims to include in the token payload, such as user identity and authorization information.
    /// </param>
    /// <returns>
    /// A tuple containing the serialized JWT string and its UTC expiration timestamp.
    /// </returns>
    public (string token, DateTime expiresAtUtc) CreateToken(IEnumerable<Claim> claims)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.ExpiresMinutes);

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expires);
    }
}
