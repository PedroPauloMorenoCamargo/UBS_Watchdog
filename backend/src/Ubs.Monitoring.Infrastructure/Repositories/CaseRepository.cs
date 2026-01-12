using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Cases;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Case aggregate operations.
/// </summary>
public sealed class CaseRepository : ICaseRepository
{
    private readonly AppDbContext _db;

    public CaseRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Case?> GetByIdAsync(Guid caseId, CancellationToken ct)
        => _db.Cases
            .AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Account)
            .Include(c => c.Analyst)
            .Include(c => c.Findings)
            .FirstOrDefaultAsync(c => c.Id == caseId, ct);

    public Task<Case?> GetByIdWithDetailsAsync(Guid caseId, CancellationToken ct)
        => _db.Cases
            .AsNoTracking()
            .Include(c => c.Transaction)
            .Include(c => c.Client)
            .Include(c => c.Account)
            .Include(c => c.Analyst)
            .Include(c => c.Findings)
                .ThenInclude(f => f.Rule)
            .FirstOrDefaultAsync(c => c.Id == caseId, ct);

    public Task<Case?> GetForUpdateAsync(Guid caseId, CancellationToken ct)
        => _db.Cases
            .Include(c => c.Findings)
            .FirstOrDefaultAsync(c => c.Id == caseId, ct);

    public Task<Case?> GetByTransactionIdAsync(Guid transactionId, CancellationToken ct)
        => _db.Cases
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TransactionId == transactionId, ct);

    public async Task<(IReadOnlyList<Case> Items, int TotalCount)> GetPagedAsync(
        CaseFilterRequest filter,
        CancellationToken ct)
    {
        var query = _db.Cases
            .AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Account)
            .Include(c => c.Analyst)
            .Include(c => c.Findings)
            .AsQueryable();

        // Apply filters
        if (filter.ClientId.HasValue)
            query = query.Where(c => c.ClientId == filter.ClientId.Value);

        if (filter.AccountId.HasValue)
            query = query.Where(c => c.AccountId == filter.AccountId.Value);

        if (filter.TransactionId.HasValue)
            query = query.Where(c => c.TransactionId == filter.TransactionId.Value);

        if (filter.AnalystId.HasValue)
            query = query.Where(c => c.AnalystId == filter.AnalystId.Value);

        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status.Value);

        if (filter.Decision.HasValue)
            query = query.Where(c => c.Decision == filter.Decision.Value);

        if (filter.Severity.HasValue)
            query = query.Where(c => c.Severity == filter.Severity.Value);

        if (filter.OpenedFrom.HasValue)
            query = query.Where(c => c.OpenedAtUtc >= filter.OpenedFrom.Value);

        if (filter.OpenedTo.HasValue)
            query = query.Where(c => c.OpenedAtUtc <= filter.OpenedTo.Value);

        if (filter.ResolvedFrom.HasValue)
            query = query.Where(c => c.ResolvedAtUtc >= filter.ResolvedFrom.Value);

        if (filter.ResolvedTo.HasValue)
            query = query.Where(c => c.ResolvedAtUtc <= filter.ResolvedTo.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Apply sorting
        query = ApplySorting(query, filter.SortBy, filter.SortDescending);

        // Apply pagination
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<CaseFinding>> GetFindingsByCaseIdAsync(
        Guid caseId,
        CaseFindingFilterRequest filter,
        CancellationToken ct)
    {
        var query = _db.CaseFindings
            .AsNoTracking()
            .Include(f => f.Rule)
            .Where(f => f.CaseId == caseId);

        // Apply sorting
        query = ApplyFindingSorting(query, filter.SortBy, filter.SortDescending);

        return await query.ToListAsync(ct);
    }

    public Task<bool> ExistsAsync(Guid caseId, CancellationToken ct)
        => _db.Cases.AnyAsync(c => c.Id == caseId, ct);

    public void Add(Case caseEntity)
    {
        ArgumentNullException.ThrowIfNull(caseEntity);
        _db.Cases.Add(caseEntity);
    }

    public void AddFinding(CaseFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        _db.CaseFindings.Add(finding);
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    private static IQueryable<Case> ApplySorting(
        IQueryable<Case> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "openedatutc" or "opened" => descending
                ? query.OrderByDescending(c => c.OpenedAtUtc)
                : query.OrderBy(c => c.OpenedAtUtc),
            "updatedatutc" or "updated" => descending
                ? query.OrderByDescending(c => c.UpdatedAtUtc)
                : query.OrderBy(c => c.UpdatedAtUtc),
            "resolvedatutc" or "resolved" => descending
                ? query.OrderByDescending(c => c.ResolvedAtUtc)
                : query.OrderBy(c => c.ResolvedAtUtc),
            "severity" => descending
                ? query.OrderByDescending(c => c.Severity)
                : query.OrderBy(c => c.Severity),
            "status" => descending
                ? query.OrderByDescending(c => c.Status)
                : query.OrderBy(c => c.Status),
            "clientname" or "client" => descending
                ? query.OrderByDescending(c => c.Client.Name)
                : query.OrderBy(c => c.Client.Name),
            _ => query.OrderByDescending(c => c.OpenedAtUtc) // Default: most recent first
        };
    }

    private static IQueryable<CaseFinding> ApplyFindingSorting(
        IQueryable<CaseFinding> query,
        string? sortBy,
        bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "severity" => descending
                ? query.OrderByDescending(f => f.Severity)
                : query.OrderBy(f => f.Severity),
            "ruletype" or "type" => descending
                ? query.OrderByDescending(f => f.RuleType)
                : query.OrderBy(f => f.RuleType),
            "rulename" or "rule" => descending
                ? query.OrderByDescending(f => f.Rule.Name)
                : query.OrderBy(f => f.Rule.Name),
            "createdatutc" or "created" => descending
                ? query.OrderByDescending(f => f.CreatedAtUtc)
                : query.OrderBy(f => f.CreatedAtUtc),
            _ => query.OrderByDescending(f => f.Severity) // Default: most severe first
        };
    }
}
