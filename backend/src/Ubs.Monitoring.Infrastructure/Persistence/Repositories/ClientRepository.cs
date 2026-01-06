using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Infrastructure.Persistence.Pagination;

namespace Ubs.Monitoring.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for client data access operations.
/// </summary>
public sealed class ClientRepository : IClientRepository
{
    private readonly AppDbContext _db;

    public ClientRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieves a client by its unique identifier.
    /// </summary>
    /// <param name="clientId">
    /// The unique identifier of the client.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The matching <see cref="Client"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Client?> GetByIdAsync(Guid clientId, CancellationToken ct)
        => _db.Clients
              .AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == clientId, ct);

    /// <summary>
    /// Retrieves a client with detailed information including related entities.
    /// </summary>
    /// <param name="clientId">
    /// The unique identifier of the client.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The client with related data if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Client?> GetByIdWithDetailsAsync(Guid clientId, CancellationToken ct)
        => _db.Clients
              .AsNoTracking()
              .Include(c => c.Accounts)
                  .ThenInclude(a => a.Identifiers)
              .Include(c => c.Transactions)
              .Include(c => c.Cases)
              .FirstOrDefaultAsync(c => c.Id == clientId, ct);

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="query">Query object containing pagination, sorting, and filter parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated result containing clients and metadata.</returns>
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

        // Apply dynamic ordering
        q = ApplyOrdering(q, query.Page.SortBy, query.Page.SortDir);

        // Use extension method to paginate (automatically handles Skip/Take/Count)
        return await q.ToPagedResultAsync(query.Page, ct);
    }

    /// <summary>
    /// Applies dynamic ordering to the query based on sortBy and sortDir parameters.
    /// </summary>
    private static IQueryable<Client> ApplyOrdering(IQueryable<Client> query, string? sortBy, string? sortDir)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query.OrderByDescending(c => c.CreatedAtUtc); // Default ordering

        var isAsc = sortDir?.ToLowerInvariant() == "asc";

        return sortBy.ToLowerInvariant() switch
        {
            "name" => isAsc ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
            "country" or "countrycode" => isAsc ? query.OrderBy(c => c.CountryCode) : query.OrderByDescending(c => c.CountryCode),
            "risk" or "risklevel" => isAsc ? query.OrderBy(c => c.RiskLevel) : query.OrderByDescending(c => c.RiskLevel),
            "kyc" or "kycstatus" => isAsc ? query.OrderBy(c => c.KycStatus) : query.OrderByDescending(c => c.KycStatus),
            "createdat" or "created" => isAsc ? query.OrderBy(c => c.CreatedAtUtc) : query.OrderByDescending(c => c.CreatedAtUtc),
            "updatedat" or "updated" => isAsc ? query.OrderBy(c => c.UpdatedAtUtc) : query.OrderByDescending(c => c.UpdatedAtUtc),
            _ => query.OrderByDescending(c => c.CreatedAtUtc) // Fallback to default
        };
    }

    /// <summary>
    /// Adds a new client to the database context.
    /// This is a synchronous operation that only modifies the in-memory change tracker.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="client">
    /// The client entity to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="client"/> is null.
    /// </exception>
    public void Add(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _db.Clients.Add(client);
    }

    /// <summary>
    /// Retrieves a client entity for update operations (tracked).
    /// </summary>
    /// <param name="clientId">
    /// The unique identifier of the client.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The tracked <see cref="Client"/> entity if found; otherwise, <c>null</c>.
    /// </returns>
    public Task<Client?> GetForUpdateAsync(Guid clientId, CancellationToken ct)
        => _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId, ct);

    /// <summary>
    /// Checks if a client with the specified ID exists.
    /// </summary>
    /// <param name="clientId">
    /// The unique identifier of the client.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// True if the client exists; otherwise, false.
    /// </returns>
    public Task<bool> ExistsAsync(Guid clientId, CancellationToken ct)
        => _db.Clients.AnyAsync(c => c.Id == clientId, ct);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">
    /// Cancellation token used to cancel the save operation.
    /// </param>
    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
