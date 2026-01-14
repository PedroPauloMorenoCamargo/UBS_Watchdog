using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.ComplianceRules;

public interface IComplianceRuleRepository
{
    Task<PagedResult<ComplianceRule>> SearchAsync(ComplianceRuleQuery query, CancellationToken ct);
    Task<ComplianceRule?> GetByIdAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    Task<IReadOnlyList<ComplianceRule>> GetActiveAsync(CancellationToken ct);
}
