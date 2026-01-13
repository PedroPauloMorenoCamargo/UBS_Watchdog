using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing compliance rules configuration.
/// </summary>
[ApiController]
[Authorize]
[Route("api/rules")]
[Produces("application/json")]
public sealed class ComplianceRulesController : ControllerBase
{
    private readonly IComplianceRuleService _service;

    public ComplianceRulesController(IComplianceRuleService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves a paginated list of compliance rules with optional filters.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100).</param>
    /// <param name="ruleType">Filter by rule type (DailyLimit, BannedCountries, BannedAccounts, Structuring).</param>
    /// <param name="isActive">Filter by active status.</param>
    /// <param name="severity">Filter by severity (Low, Medium, High, Critical).</param>
    /// <param name="scope">Filter by scope (e.g., country code or 'global').</param>
    /// <param name="sortBy">Sort field (UpdatedAtUtc, Name, RuleType, Severity).</param>
    /// <param name="sortDir">Sort direction (asc or desc).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of compliance rules.</returns>
    /// <response code="200">Returns the paginated list of rules.</response>
    /// <response code="400">Invalid sort field or direction.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ComplianceRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] int page = PaginationDefaults.DefaultPage,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize,
        [FromQuery] RuleType? ruleType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Severity? severity = null,
        [FromQuery] string? scope = null,
        [FromQuery] string sortBy = "UpdatedAtUtc",
        [FromQuery] string sortDir = "desc",
        CancellationToken ct = default)
    {
        if (!ComplianceRuleSortFields.IsValid(sortBy))
            return Problem(title: "Invalid sort field", statusCode: StatusCodes.Status400BadRequest);

        if (!string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase))
            return Problem(title: "Invalid sort direction", statusCode: StatusCodes.Status400BadRequest);

        var result = await _service.SearchAsync(
            new ComplianceRuleQuery
            {
                Page = new PageRequest { Page = page, PageSize = pageSize, SortBy = sortBy, SortDir = sortDir },
                RuleType = ruleType,
                IsActive = isActive,
                Severity = severity,
                Scope = scope
            },
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific compliance rule by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the rule.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The compliance rule details.</returns>
    /// <response code="200">Returns the rule details.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Rule not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _service.GetByIdAsync(id, ct);
        if (rule is null)
            return Problem(title: "Rule not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(rule);
    }

    /// <summary>
    /// Updates a compliance rule's configuration.
    /// </summary>
    /// <param name="id">The unique identifier of the rule.</param>
    /// <param name="req">The update request with fields to modify.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated compliance rule.</returns>
    /// <response code="200">Rule updated successfully.</response>
    /// <response code="400">Invalid request, no changes provided, or invalid parameters.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Rule not found.</response>
    /// <remarks>
    /// Only provided fields will be updated. All fields are optional:
    /// - **Name**: Display name of the rule
    /// - **IsActive**: Enable or disable the rule
    /// - **Severity**: Alert severity when rule is violated
    /// - **Scope**: Country code or 'global'
    /// - **Parameters**: Rule-specific JSON configuration
    /// </remarks>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchRuleRequest req, CancellationToken ct)
    {
        if (req is null)
            return Problem(title: "Invalid payload", statusCode: StatusCodes.Status400BadRequest);

        var result = await _service.PatchAsync(id, new PatchComplianceRuleData(req.Name, req.IsActive, req.Severity, req.Scope, req.Parameters), ct);

        return result.Status switch
        {
            PatchComplianceRuleStatus.NoChanges => Problem(title: "No changes provided", statusCode: StatusCodes.Status400BadRequest),

            PatchComplianceRuleStatus.NotFound => Problem(title: "Rule not found", statusCode: StatusCodes.Status404NotFound),

            PatchComplianceRuleStatus.InvalidParameters =>
                Problem(
                    title: "Invalid rule parameters",
                    detail: string.Join(" ", result.Errors ?? Array.Empty<string>()),
                    statusCode: StatusCodes.Status400BadRequest),

            PatchComplianceRuleStatus.InvalidUpdate =>
                Problem(
                    title: "Invalid rule update",
                    detail: string.Join(" ", result.Errors ?? Array.Empty<string>()),
                    statusCode: StatusCodes.Status400BadRequest),

            PatchComplianceRuleStatus.Success => Ok(result.Rule),

            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
