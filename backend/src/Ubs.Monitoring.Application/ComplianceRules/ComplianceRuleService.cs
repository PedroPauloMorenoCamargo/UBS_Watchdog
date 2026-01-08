using System.Text.Json;
using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.ComplianceRules;

public sealed class ComplianceRuleService : IComplianceRuleService
{
    private readonly IComplianceRuleRepository _repo;
    private readonly IComplianceRuleParametersValidator _validator;

    public ComplianceRuleService(
        IComplianceRuleRepository repo,
        IComplianceRuleParametersValidator validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<PagedResult<ComplianceRuleDto>> SearchAsync(ComplianceRuleQuery query, CancellationToken ct)
    {
        var paged = await _repo.SearchAsync(query, ct);
        return paged.Map(ToDto);
    }

    public async Task<ComplianceRuleDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var rule = await _repo.GetByIdAsync(id, ct);
        return rule is null ? null : ToDto(rule);
    }

    public async Task<PatchComplianceRuleResult> PatchAsync(Guid id, PatchComplianceRuleData patch, CancellationToken ct)
    {
        var hasAny =
            patch.Name is not null ||
            patch.IsActive is not null ||
            patch.Severity is not null ||
            patch.Scope is not null ||
            patch.Parameters is not null;

        if (!hasAny)
            return new PatchComplianceRuleResult(PatchComplianceRuleStatus.NoChanges);

        var rule = await _repo.GetByIdAsync(id, ct);
        if (rule is null)
            return new PatchComplianceRuleResult(PatchComplianceRuleStatus.NotFound);

        try
        {
            if (patch.Name is not null) rule.Rename(patch.Name);
            if (patch.IsActive is not null) rule.SetActive(patch.IsActive.Value);
            if (patch.Severity is not null) rule.UpdateSeverity(patch.Severity.Value);
            if (patch.Scope is not null) rule.UpdateScope(patch.Scope);

            if (patch.Parameters is not null)
            {
                var errors = await _validator.ValidateAsync(rule.RuleType, patch.Parameters.Value, ct);
                if (errors.Count > 0)
                    return new PatchComplianceRuleResult(PatchComplianceRuleStatus.InvalidParameters, Errors: errors);

                var json = JsonSerializer.Serialize(patch.Parameters.Value);
                rule.UpdateParametersJson(json);
            }

            await _repo.SaveChangesAsync(ct);
            return new PatchComplianceRuleResult(PatchComplianceRuleStatus.Success, Rule: ToDto(rule));
        }
        catch (ArgumentException ex)
        {
            return new PatchComplianceRuleResult(
                PatchComplianceRuleStatus.InvalidUpdate,
                Errors: new[] { ex.Message }
            );
        }
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
