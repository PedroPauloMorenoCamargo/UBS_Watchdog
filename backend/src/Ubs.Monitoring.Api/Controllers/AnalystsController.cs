using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Application.Analysts;

namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/analysts")]
[Produces("application/json")]
public sealed class AnalystsController : ControllerBase
{
    private readonly IAnalystProfileRepository _analystRead;
    private readonly IAnalystProfileService _profile;

    public AnalystsController( IAnalystProfileRepository analystRead, IAnalystProfileService profile)
    {
        _analystRead = analystRead;
        _profile = profile;
    }
    /// <summary>
    /// Retrieve an analyst profile by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AnalystProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnalystProfileResponse>> GetById(Guid id, CancellationToken ct)
    {
        var a = await _analystRead.GetByIdAsync(id, ct);
        if (a is null)
        {
            return Problem(
                title: "Analyst not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(new AnalystProfileResponse(
            a.Id,
            a.CorporateEmail,
            a.FullName,
            a.PhoneNumber,
            a.ProfilePictureBase64,
            a.CreatedAtUtc
        ));
    }
    /// <summary>
    /// Update or clear the authenticated analyst's profile picture.
    /// </summary>
    [HttpPatch("me/profile-picture")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfilePicture(
        [FromBody] UpdateProfilePictureRequest req,
        CancellationToken ct)
    {
        
        if (!TryGetAnalystId(out var analystId))
        {
            return Problem(title: "Unauthorized", statusCode: StatusCodes.Status401Unauthorized);
        }

        try
        {
            var updated = await _profile.UpdateProfilePictureAsync(
                analystId,
                req.ProfilePictureBase64,
                ct
            );

            if (!updated)
            {
                return Problem(
                    title: "Analyst not found",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid profile picture",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }
    /// <summary>
    /// Extracts the authenticated analyst's ID from JWT claims.
    /// This method assumes [Authorize] has already validated authentication.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the authenticated principal is missing required claims.
    /// </exception>
    private bool TryGetAnalystId(out Guid id)
    {
        var raw =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out id);
    }
}
