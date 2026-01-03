using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.ComplianceRules;

public interface IComplianceRuleService
{
    Task<PagedResult<ComplianceRuleDto>> SearchAsync(ComplianceRuleQuery query, CancellationToken ct);
    Task<ComplianceRuleDto?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<PatchComplianceRuleResult> PatchAsync(Guid id, PatchComplianceRuleData patch, CancellationToken ct);
}
