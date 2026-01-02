using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Analysts;
using Ubs.Monitoring.Application.Auth;

namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/analysts")]
public sealed class AnalystsController : ControllerBase
{
    private readonly IAnalystRepository _analystRead;
    private readonly IAnalystProfileService _profile;

    public AnalystsController(IAnalystRepository analystRead, IAnalystProfileService profile)
    {
        _analystRead = analystRead;
        _profile = profile;
    }

    public sealed record UpdateProfilePictureRequest(string? ProfilePictureBase64);

    /// <summary>
    /// Retrieve an analyst profile by its unique identifier.
    /// </summary>
    /// <param name="id">The UUID of the analyst.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The analyst profile if found; otherwise, a 404 ProblemDetails response.
    /// </returns>
    /// <response code="200">Analyst profile retrieved successfully.</response>
    /// <response code="404">Analyst not found.</response>
    /// <response code="401">Request is unauthorized.</response>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnalystProfileDto>> GetById(Guid id, CancellationToken ct)
    {
        var a = await _analystRead.GetByIdAsync(id, ct);
        if (a is null)
        {
            return Problem(
                title: "Analyst not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(new AnalystProfileDto(
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
    /// <remarks>
    /// Business rules:
    ///     - Only the authenticated analyst can update their own profile picture
    ///     - Payload must be valid Base64 and within size limits
    ///     - Other analyst fields are immutable via this endpoint
    /// </remarks>
    /// <param name="req">Profile picture update payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 No Content on success; appropriate ProblemDetails on failure.
    /// </returns>
    /// <response code="204">Profile picture updated successfully.</response>
    /// <response code="400">Invalid Base64 payload or size violation.</response>
    /// <response code="401">Request is unauthorized.</response>
    /// <response code="404">Analyst not found.</response>
    [HttpPut("me/profile-picture")]
    public async Task<IActionResult> UpdateMyProfilePicture(
        [FromBody] UpdateProfilePictureRequest req,
        CancellationToken ct
    )
    {
        var analystId = GetAnalystIdOrNull();
        if (analystId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        try
        {
            var ok = await _profile.UpdateProfilePictureAsync(
                analystId.Value,
                req.ProfilePictureBase64,
                ct
            );

            if (!ok)
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
                title: "Invalid input",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }

    /// <summary>
    /// Extract the authenticated analyst's ID from the JWT claims.
    /// </summary>
    /// <returns>
    /// Analyst UUID if present and valid; otherwise, null.
    /// </returns>
    private Guid? GetAnalystIdOrNull()
    {
        var sub = User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
