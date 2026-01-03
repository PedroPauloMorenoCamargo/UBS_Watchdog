using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Api.Contracts;
namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Route("api/auth")]
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
    /// A <see cref="LoginResultDto"/> containing a JWT token, expiration metadata,
    /// and the authenticated analyst profile if the credentials are valid.
    ///
    /// If authentication fails, returns an RFC 7807 problem response with HTTP 401 Unauthorized.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req,
        CancellationToken ct)
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

        return Ok(new LoginResponse(
            result.Token,
            result.ExpiresAtUtc,
            new AnalystProfileResponse(
                result.Analyst.Id,
                result.Analyst.CorporateEmail,
                result.Analyst.FullName,
                result.Analyst.PhoneNumber,
                result.Analyst.ProfilePictureBase64,
                result.Analyst.CreatedAtUtc
            )
        ));
    }


    /// <summary>
    /// Retrieves the authenticated analyst's profile information.
    /// </summary>
    /// <param name="ct">
    /// Cancellation token used to cancel the request.
    /// </param>
    /// <returns>
    /// The authenticated analyst's profile if the JWT is valid and the analyst exists.
    ///
    /// Returns HTTP 401 Unauthorized if the token is invalid or missing required claims, or HTTP 404 Not Found if the analyst no longer exists.
    /// </returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(AnalystProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var analystId = GetAnalystIdOrNull();
        if (analystId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var me = await _auth.GetMeAsync(analystId.Value, ct);
        if (me is null)
        {
            return Problem(
                title: "Analyst not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(new AnalystProfileResponse(
            me.Id,
            me.CorporateEmail,
            me.FullName,
            me.PhoneNumber,
            me.ProfilePictureBase64,
            me.CreatedAtUtc
        ));
    }


    /// <summary>
    /// Logs the analyst out of the system.
    /// </summary>
    /// <returns>
    /// HTTP 204 No Content.
    ///
    /// This endpoint does not invalidate the JWT on the server side.
    /// Logout is stateless and requires the client to discard the token.
    /// </returns>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout() => NoContent();

    /// <summary>
    /// Extracts the authenticated analyst identifier from the current JWT claims.
    /// </summary>
    /// <returns>
    /// The analyst identifier if present and valid; otherwise, <c>null</c>.
    /// </returns>
    private Guid? GetAnalystIdOrNull()
    {
        var sub = User.FindFirstValue("sub")
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
