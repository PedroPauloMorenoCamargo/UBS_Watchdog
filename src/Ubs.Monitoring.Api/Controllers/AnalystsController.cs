using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Mappers;
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

    public AnalystsController(
        IAnalystProfileRepository analystRead,
        IAnalystProfileService profile)
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
        var dto = await _analystRead.GetProfileByIdAsync(id, ct);
        if (dto is null)
        {
            return Problem(
                title: "Analyst not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(AnalystProfileMapper.ToResponse(dto));
    }

    /// <summary>
    /// Updates or clears the authenticated analyst's profile picture.
    /// </summary>
    /// <param name="req">Request containing the new profile picture (base64) or null to clear.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Profile picture updated successfully.</response>
    /// <response code="400">Validation failed (base64 invalid/too large).</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Analyst not found.</response>
    [HttpPatch("me/profile-picture")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfilePicture([FromBody] UpdateProfilePictureRequest req, CancellationToken ct)
    {
        var analystId = GetAnalystIdOrNull();
        if (analystId is null)
        {
            return Problem(
                title: "Unauthorized",
                detail: "Authenticated user is missing a valid identifier claim.",
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        var updated = await _profile.UpdateProfilePictureAsync(
            analystId.Value,
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

    private Guid? GetAnalystIdOrNull()
    {
        var raw =
            User.FindFirstValue("sub") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
