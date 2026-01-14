using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Mappers;
using Ubs.Monitoring.Application.Auth;

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
    /// The login request containing the analyst's email and password.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A <see cref="LoginResponse"/> containing a JWT access token, expiration metadata, and the authenticated analyst profile if the credentials
    /// are valid.
    /// </returns>
    /// <response code="200">Authentication succeeded and a JWT token was issued.</response>
    /// <response code="400">Invalid request payload.</response>
    /// /// <response code="401">Authentication failed due to invalid credentials.</response>
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
    /// Retrieves the profile of the currently authenticated analyst.
    /// </summary>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The authenticated analyst profile derived from the JWT identity.
    /// </returns>
    /// <response code="200">Returns the authenticated analyst profile.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Authenticated analyst not found.</response>
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
    /// Logs the currently authenticated analyst out of the system.
    /// </summary>
    /// <remarks>
    /// This endpoint is stateless and does not invalidate JWT tokens server-side. Clients are responsible for discarding the token.
    /// </remarks>
    /// <response code="204">Logout completed successfully.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
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
