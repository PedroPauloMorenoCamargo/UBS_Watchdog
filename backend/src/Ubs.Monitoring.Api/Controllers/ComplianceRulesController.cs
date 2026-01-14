using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Mappers;
using Ubs.Monitoring.Application.ComplianceRules;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing compliance rules configuration.
/// </summary>
[ApiController]
[Authorize]
[Route("api/rules")]
[Produces("application/json", "application/problem+json")]
public sealed class ComplianceRulesController : ControllerBase
{
    private readonly IComplianceRuleService _service;

    public ComplianceRulesController(IComplianceRuleService service)
    {
        _service = service;
    }

    /// <summary>
    /// Searches compliance rules using pagination, sorting, and optional filtering criteria.
    /// </summary>
    /// <param name="req">
    /// The search request containing pagination, sorting, and filter parameters.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A paged collection of compliance rules matching the provided criteria.
    /// </returns>
    /// <response code="200">Returns the paged list of compliance rules.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ComplianceRuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] SearchComplianceRulesRequest req, CancellationToken ct)
    {
        var query = ComplianceRuleContractMapper.ToQuery(req);
        var result = await _service.SearchAsync(query, ct);

        return Ok(ComplianceRuleContractMapper.ToPagedResponse(result));
    }

    /// <summary>
    /// Retrieves a single compliance rule by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the compliance rule.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The compliance rule if found.
    /// </returns>
    /// <response code="200">Returns the compliance rule.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Compliance rule not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _service.GetByIdAsync(id, ct);
        if (rule is null)
        {
            return Problem(
                title: "Compliance rule not found",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(ComplianceRuleContractMapper.ToResponse(rule));
    }

    /// <summary>
    /// Applies a partial update to an existing compliance rule.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the compliance rule to update.
    /// </param>
    /// <param name="req">
    /// The patch request containing the fields to update.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The updated compliance rule if the patch operation succeeds.
    /// </returns>
    /// <response code="200">Compliance rule updated successfully.</response>
    /// <response code="400">
    /// Invalid update request, no changes applied, or invalid rule parameters.
    /// </response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Compliance rule not found.</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchComplianceRuleRequest req, CancellationToken ct)
    {
        // Validator enforces "at least one field to update"
        var patch = ComplianceRuleContractMapper.ToPatchDto(req);
        var result = await _service.PatchAsync(id, patch, ct);

        return result.Status switch
        {
            PatchComplianceRuleStatus.NotFound =>
                Problem(title: "Compliance rule not found", statusCode: StatusCodes.Status404NotFound),

            PatchComplianceRuleStatus.NoChanges =>
                Problem(
                    title: "No changes applied",
                    detail: "The submitted update did not change any values.",
                    statusCode: StatusCodes.Status400BadRequest),

            PatchComplianceRuleStatus.InvalidParameters =>
                Problem(
                    title: "Invalid parameters",
                    detail: string.Join(" ", result.Errors ?? Array.Empty<string>()),
                    statusCode: StatusCodes.Status400BadRequest),

            PatchComplianceRuleStatus.InvalidUpdate =>
                Problem(
                    title: "Invalid rule update",
                    detail: string.Join(" ", result.Errors ?? Array.Empty<string>()),
                    statusCode: StatusCodes.Status400BadRequest),

            PatchComplianceRuleStatus.Success =>
                Ok(ComplianceRuleContractMapper.ToResponse(result.Rule!)),

            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
