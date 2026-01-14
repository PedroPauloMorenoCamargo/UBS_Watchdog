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
    /// <summary>
    /// Searches compliance rules using pagination and optional filtering criteria.
    /// </summary>
    /// <param name="q">
    /// The query object containing filter conditions, pagination, and sort options.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A <see cref="PagedResult{T}"/> containing compliance rules that match the specified query criteria.
    /// </returns>
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
    /// <summary>
    /// Retrieves a compliance rule by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the compliance rule.
    /// </param>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The <see cref="ComplianceRule"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<ComplianceRule?> GetByIdAsync(Guid id, CancellationToken ct)  => _db.ComplianceRules.FirstOrDefaultAsync(x => x.Id == id, ct);
    /// <summary>
    /// Persists pending changes to the underlying database.
    /// </summary>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A task that completes when changes have been successfully saved.
    /// </returns>
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    /// <summary>
    /// Retrieves all active compliance rules.
    /// </summary>
    /// <param name="ct">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// A read-only list of active <see cref="ComplianceRule"/> entities.
    public async Task<IReadOnlyList<ComplianceRule>> GetActiveAsync(CancellationToken ct) => await _db.ComplianceRules
        .AsNoTracking()
        .Where(x => x.IsActive)
        .ToListAsync(ct);

    /// <summary>
    /// Applies entity-specific sorting to the compliance rule query.
    /// </summary>
    /// <remarks>
    /// Only a predefined set of sortable fields is supported to ensure
    /// predictable query behavior and prevent invalid or unsafe ordering.
    /// </remarks>
    /// <param name="query">
    /// The base compliance rule query.
    /// </param>
    /// <param name="sortBy">
    /// The field name used for sorting.
    /// </param>
    /// <param name="sortDir">
    /// The sort direction (<c>asc</c> or <c>desc</c>).
    /// </param>
    /// <returns>
    /// The query with ordering applied.
    /// </returns>
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
