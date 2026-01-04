using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence.Pagination;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

public sealed class ComplianceRuleRepository : IComplianceRuleRepository
{
    private readonly AppDbContext _db;

    public ComplianceRuleRepository(AppDbContext db) => _db = db;

    public async Task<PagedResult<ComplianceRule>> SearchAsync(ComplianceRuleQuery q, CancellationToken ct)
    {
        IQueryable<ComplianceRule> query = _db.ComplianceRules.AsNoTracking();

        if (q.RuleType is not null) query = query.Where(x => x.RuleType == q.RuleType);
        if (q.IsActive is not null) query = query.Where(x => x.IsActive == q.IsActive);
        if (q.Severity is not null) query = query.Where(x => x.Severity == q.Severity);
        if (!string.IsNullOrWhiteSpace(q.Scope)) query = query.Where(x => x.Scope == q.Scope);

        // Strict, per-entity sort mapping (best practice)
        query = ApplySort(query, q.Page.SortBy, q.Page.SortDir);

        // Common paging implementation (normalizes page/pageSize)
        return await query.ToPagedResultAsync(q.Page, ct);
    }

    public Task<ComplianceRule?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.ComplianceRules.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private static IQueryable<ComplianceRule> ApplySort(IQueryable<ComplianceRule> query, string? sortBy, string? sortDir)
    {
        var sb = (sortBy ?? "UpdatedAtUtc").Trim();
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        return sb.ToLowerInvariant() switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "ruletype" => desc ? query.OrderByDescending(x => x.RuleType) : query.OrderBy(x => x.RuleType),
            "severity" => desc ? query.OrderByDescending(x => x.Severity) : query.OrderBy(x => x.Severity),
            "createdatutc" => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            "updatedatutc" => desc ? query.OrderByDescending(x => x.UpdatedAtUtc) : query.OrderBy(x => x.UpdatedAtUtc),
            _ => desc ? query.OrderByDescending(x => x.UpdatedAtUtc) : query.OrderBy(x => x.UpdatedAtUtc),
        };
    }
}
