using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence.Pagination;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly AppDbContext _db;

    public ClientRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Client?> GetByIdAsync(Guid clientId, CancellationToken ct)
        => _db.Clients
              .AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == clientId, ct);

    public Task<Client?> GetByIdWithDetailsAsync(Guid clientId, CancellationToken ct)
        => _db.Clients
              .AsNoTracking()
              .Include(c => c.Accounts)
                  .ThenInclude(a => a.Identifiers)
              .Include(c => c.Transactions)
              .Include(c => c.Cases)
              .FirstOrDefaultAsync(c => c.Id == clientId, ct);

    public async Task<PagedResult<Client>> GetPagedAsync(ClientQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var q = _db.Clients.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.CountryCode))
        {
            var normalizedCountry = query.CountryCode.ToUpperInvariant();
            q = q.Where(c => c.CountryCode == normalizedCountry);
        }

        if (query.RiskLevel.HasValue)
        {
            q = q.Where(c => c.RiskLevel == query.RiskLevel.Value);
        }

        if (query.KycStatus.HasValue)
        {
            q = q.Where(c => c.KycStatus == query.KycStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLowerInvariant();
            q = q.Where(c => c.Name.ToLower().Contains(searchTerm));
        }

        // Apply dynamic ordering
        q = ApplyOrdering(q, query.Page.SortBy, query.Page.SortDir);

        // Use extension method to paginate (automatically handles Skip/Take/Count)
        return await q.ToPagedResultAsync(query.Page, ct);
    }

    /// <summary>
    /// Sort field names must match property names exactly (case-insensitive).
    /// </summary>
    private static IQueryable<Client> ApplyOrdering(IQueryable<Client> query, string? sortBy, string? sortDir)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(c => c.CreatedAtUtc); // Default ordering

        var isAsc = sortDir?.ToLowerInvariant() == "asc";

        return sortBy.ToLowerInvariant() switch
        {
            "name" => isAsc ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
            "countrycode" => isAsc ? query.OrderBy(c => c.CountryCode) : query.OrderByDescending(c => c.CountryCode),
            "risklevel" => isAsc ? query.OrderBy(c => c.RiskLevel) : query.OrderByDescending(c => c.RiskLevel),
            "kycstatus" => isAsc ? query.OrderBy(c => c.KycStatus) : query.OrderByDescending(c => c.KycStatus),
            "createdatutc" => isAsc ? query.OrderBy(c => c.CreatedAtUtc) : query.OrderByDescending(c => c.CreatedAtUtc),
            "updatedatutc" => isAsc ? query.OrderBy(c => c.UpdatedAtUtc) : query.OrderByDescending(c => c.UpdatedAtUtc),
            _ => query.OrderByDescending(c => c.CreatedAtUtc) // Fallback to default
        };
    }

    public void Add(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _db.Clients.Add(client);
    }

    public Task<Client?> GetForUpdateAsync(Guid clientId, CancellationToken ct)
        => _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId, ct);

    public Task<bool> ExistsAsync(Guid clientId, CancellationToken ct)
        => _db.Clients.AnyAsync(c => c.Id == clientId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
