using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

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
    /// Retrieves a paginated list of clients with optional filters.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="countryCode">Optional country code filter.</param>
    /// <param name="riskLevel">Optional risk level filter.</param>
    /// <param name="kycStatus">Optional KYC status filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the paginated list of clients and the total count.
    /// </returns>
    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? countryCode = null,
        RiskLevel? riskLevel = null,
        KycStatus? kycStatus = null,
        CancellationToken ct = default)
    {
        var query = _db.Clients.AsNoTracking();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            var normalizedCountry = countryCode.ToUpperInvariant();
            query = query.Where(c => c.CountryCode == normalizedCountry);
        }

        if (riskLevel.HasValue)
        {
            query = query.Where(c => c.RiskLevel == riskLevel.Value);
        }

        if (kycStatus.HasValue)
        {
            query = query.Where(c => c.KycStatus == kycStatus.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <summary>
    /// Adds a new client to the database.
    /// </summary>
    /// <param name="client">
    /// The client entity to add.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    public async Task AddAsync(Client client, CancellationToken ct)
    {
        await _db.Clients.AddAsync(client, ct);
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
