using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Repository interface for client data access operations.
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Retrieves a client by their unique identifier.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client if found; otherwise, null.</returns>
    Task<Client?> GetByIdAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Retrieves a client with detailed information including related entities.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client with related data if found; otherwise, null.</returns>
    Task<Client?> GetByIdWithDetailsAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="query">Query object containing pagination, sorting, and filter parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated result containing clients and metadata.</returns>
    Task<PagedResult<Client>> GetPagedAsync(ClientQuery query, CancellationToken ct);

    /// <summary>
    /// Adds a new client to the database context.
    /// This is a synchronous operation that only modifies the in-memory change tracker.
    /// Call SaveChangesAsync to persist changes to the database.
    /// </summary>
    /// <param name="client">The client entity to add.</param>
    void Add(Client client);

    /// <summary>
    /// Retrieves a client entity for update operations (tracked).
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked client entity if found; otherwise, null.</returns>
    Task<Client?> GetForUpdateAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Checks if a client with the specified ID exists.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the client exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken ct);
}
