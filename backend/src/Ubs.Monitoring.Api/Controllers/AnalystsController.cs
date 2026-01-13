using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Application.Analysts;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing analyst profiles.
/// </summary>
[ApiController]
[Authorize]
[Route("api/analysts")]
[Produces("application/json")]
public sealed class AnalystsController : ControllerBase
{
    private readonly IAnalystProfileRepository _analystRead;
    private readonly IAnalystProfileService _profile;

    public AnalystsController(IAnalystProfileRepository analystRead, IAnalystProfileService profile)
    {
        _analystRead = analystRead;
        _profile = profile;
    }

    /// <summary>
    /// Retrieves an analyst profile by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the analyst.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The analyst profile information.</returns>
    /// <response code="200">Returns the analyst profile.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Analyst not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AnalystProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    /// Updates or clears the authenticated analyst's profile picture.
    /// </summary>
    /// <param name="req">Request containing the new profile picture (base64) or null to clear.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Profile picture updated successfully.</response>
    /// <response code="400">Invalid profile picture format or size.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Analyst not found.</response>
    /// <remarks>
    /// Send a base64-encoded image to update, or null to clear the profile picture.
    /// Supported formats: JPEG, PNG. Maximum size: 1MB.
    /// </remarks>
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

    private bool TryGetAnalystId(out Guid id)
    {
        var raw =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out id);
    }
}
