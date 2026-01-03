using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Controllers;

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

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ComplianceRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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

        if (scope is not null && !ComplianceRuleScopes.IsValid(scope))
            return Problem(title: "Invalid scope", statusCode: StatusCodes.Status400BadRequest);

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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _service.GetByIdAsync(id, ct);
        if (rule is null)
            return Problem(title: "Rule not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(rule);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ComplianceRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
