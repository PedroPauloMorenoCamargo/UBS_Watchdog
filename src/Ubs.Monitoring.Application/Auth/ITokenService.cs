using System.Security.Claims;

namespace Ubs.Monitoring.Application.Auth;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) CreateToken(IEnumerable<Claim> claims);
}
