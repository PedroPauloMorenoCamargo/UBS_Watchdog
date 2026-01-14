using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Service interface for client business operations.
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Creates a new client.
    /// </summary>
    /// <param name="request">The client creation request data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the created client data and error message.
    /// If successful, Result contains the client data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error from the domain.
    /// </returns>
    Task<(ClientResponseDto? Result, string? ErrorMessage)> CreateClientAsync(CreateClientRequest request, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="query">Query object containing pagination, sorting, and filter parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated result containing client DTOs and metadata.</returns>
    Task<PagedResult<ClientResponseDto>> GetPagedClientsAsync(ClientQuery query, CancellationToken ct);

    /// <summary>
    /// Retrieves detailed information about a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client details if found; otherwise, null.</returns>
    Task<ClientDetailDto?> GetClientByIdAsync(Guid clientId, CancellationToken ct);

    /// <summary>
    /// Imports multiple clients from a CSV or Excel file.
    /// </summary>
    /// <param name="fileStream">The file stream containing client data.</param>
    /// <param name="fileName">The file name (used to determine format).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Import result with success count and errors.</returns>
    Task<ImportResultDto> ImportClientsFromFileAsync(Stream fileStream, string fileName, CancellationToken ct);
}
