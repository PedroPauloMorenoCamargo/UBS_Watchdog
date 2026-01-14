using System.Text.Json;
using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.ComplianceRules;

public sealed class ComplianceRuleService : IComplianceRuleService
{
    private readonly IComplianceRuleRepository _repo;
    private readonly IComplianceRuleParametersValidator _validator;

    public ComplianceRuleService( IComplianceRuleRepository repo, IComplianceRuleParametersValidator validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public async Task<PagedResult<ComplianceRuleDto>> SearchAsync(ComplianceRuleQuery query, CancellationToken ct)
    {
        var paged = await _repo.SearchAsync(query, ct);
        return paged.Map(ComplianceRuleMapper.ToDto);
    }

    public async Task<ComplianceRuleDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var rule = await _repo.GetByIdAsync(id, ct);
        return rule is null ? null : ComplianceRuleMapper.ToDto(rule);
    }

    public async Task<PatchComplianceRuleResult> PatchAsync( Guid id, PatchComplianceRuleDto patch, CancellationToken ct)
    {
        var rule = await _repo.GetByIdAsync(id, ct);
        if (rule is null)
            return new PatchComplianceRuleResult(PatchComplianceRuleStatus.NotFound);

        var changed = false;

        try
        {
            if (patch.Name is not null && patch.Name != rule.Name)
            {
                rule.Rename(patch.Name);
                changed = true;
            }

            if (patch.IsActive is not null && patch.IsActive.Value != rule.IsActive)
            {
                rule.SetActive(patch.IsActive.Value);
                changed = true;
            }

            if (patch.Severity is not null && patch.Severity.Value != rule.Severity)
            {
                rule.UpdateSeverity(patch.Severity.Value);
                changed = true;
            }

            if (patch.Scope is not null && patch.Scope != rule.Scope)
            {
                rule.UpdateScope(patch.Scope);
                changed = true;
            }

            if (patch.Parameters is not null)
            {
                var errors = await _validator.ValidateAsync(rule.RuleType, patch.Parameters.Value, ct);
                if (errors.Count > 0)
                    return new PatchComplianceRuleResult(
                        PatchComplianceRuleStatus.InvalidParameters,
                        Errors: errors);

                var json = JsonSerializer.Serialize(patch.Parameters.Value);

                if (!string.Equals(json, rule.ParametersJson, StringComparison.Ordinal))
                {
                    rule.UpdateParametersJson(json);
                    changed = true;
                }
            }

            if (!changed)
                return new PatchComplianceRuleResult(PatchComplianceRuleStatus.NoChanges);

            await _repo.SaveChangesAsync(ct);

            return new PatchComplianceRuleResult(
                PatchComplianceRuleStatus.Success,
                Rule: ComplianceRuleMapper.ToDto(rule));
        }
        catch (ArgumentException ex)
        {
            return new PatchComplianceRuleResult(
                PatchComplianceRuleStatus.InvalidUpdate,
                Errors: new[] { ex.Message });
        }
    }

}
