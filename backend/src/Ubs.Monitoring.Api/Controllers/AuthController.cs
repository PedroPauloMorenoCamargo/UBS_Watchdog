using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Mappers;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Application.Analysts;

namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json", "application/problem+json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Authenticates an analyst using email and password credentials.
    /// </summary>
    /// <param name="req">
    /// The login request containing the analyst's credentials.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the authentication request.
    /// </param>
    /// <returns>
    /// A <see cref="LoginResponse"/> containing a JWT token, expiration metadata, and the authenticated analyst profile if the credentials are valid.
    /// If authentication fails, returns an RFC 7807 problem response with HTTP 401 Unauthorized.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login( [FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(req.Email, req.Password, ct);
        if (result is null)
        {
            return Problem(
                title: "Invalid credentials",
                detail: "Email or password is incorrect.",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        return Ok(AuthContractMapper.ToLoginResponse(result));
    }

    /// <summary>
    /// Retrieves the authenticated analyst's profile information.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(AnalystProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalystProfileResponse>> Me(CancellationToken ct)
    {
        var analystId = GetAnalystIdOrNull();
        if (analystId is null)
        {
            // Fallback (should not happen since its protected by Authorize)
            return Problem(
                title: "Unauthorized",
                detail: "Missing required identity claim.",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var me = await _auth.GetMeAsync(analystId.Value, ct);
        if (me is null)
        {
            return Problem(
                title: "Analyst not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(AuthContractMapper.ToAnalystProfileResponse(me));
    }

    /// <summary>
    /// Logs the analyst out of the system (stateless).
    /// </summary>
    /// <remarks>
    /// This endpoint does not invalidate JWTs server-side.
    /// </remarks>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Logout() => NoContent();

    private Guid? GetAnalystIdOrNull()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
