using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Api.Contracts;


namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/rules")]
public sealed class ComplianceRulesController : ControllerBase
{
    private readonly IComplianceRuleRepository _repo;
    private readonly IComplianceRuleParametersValidator _validator;

    public ComplianceRulesController(IComplianceRuleRepository repo, IComplianceRuleParametersValidator validator)
    {
        _repo = repo;
        _validator = validator;
    }

    [HttpGet]
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

        var result = await _repo.SearchAsync(
            new ComplianceRuleQuery
            {
                Page = new PageRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDir = sortDir
                },
                RuleType = ruleType,
                IsActive = isActive,
                Severity = severity,
                Scope = scope
            },
            ct
        );

        return Ok(result.Map(ToDto).ToPagedResponse());
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var rule = await _repo.GetByIdAsync(id, ct);
        if (rule is null)
            return Problem(title: "Rule not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(ToDto(rule));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchRuleRequest req, CancellationToken ct)
    {
        if (req is null)
            return Problem(title: "Invalid payload", statusCode: StatusCodes.Status400BadRequest);

        var hasAny =
            req.Name is not null ||
            req.IsActive is not null ||
            req.Severity is not null ||
            req.Scope is not null ||
            req.Parameters is not null;

        if (!hasAny)
            return Problem(title: "No changes provided", statusCode: StatusCodes.Status400BadRequest);

        var rule = await _repo.GetByIdAsync(id, ct);
        if (rule is null)
            return Problem(title: "Rule not found", statusCode: StatusCodes.Status404NotFound);
        try
        {
            if (req.Name is not null) rule.Rename(req.Name);
            if (req.IsActive is not null) rule.SetActive(req.IsActive.Value);
            if (req.Severity is not null) rule.UpdateSeverity(req.Severity.Value);
            if (req.Scope is not null) rule.UpdateScope(req.Scope);

            if (req.Parameters is not null)
            {
                var errors = _validator.Validate(rule.RuleType, req.Parameters.Value);
                if (errors.Count > 0)
                {
                    return Problem(
                        title: "Invalid rule parameters",
                        detail: string.Join(" ", errors),
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var json = JsonSerializer.Serialize(req.Parameters.Value);
                rule.UpdateParametersJson(json);
            }
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid rule update",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }


        await _repo.SaveChangesAsync(ct);
        return Ok(ToDto(rule));
    }

    private static ComplianceRuleDto ToDto(Domain.Entities.ComplianceRule r)
    {
        using var doc = JsonDocument.Parse(r.ParametersJson);
        var parameters = doc.RootElement.Clone();

        return new ComplianceRuleDto(
            r.Id,
            r.Code,
            r.RuleType,
            r.Name,
            r.IsActive,
            r.Severity,
            r.Scope,
            parameters,
            r.CreatedAtUtc,
            r.UpdatedAtUtc
        );
    }
}
