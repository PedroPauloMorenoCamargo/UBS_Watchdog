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
    /// <returns>The created client data if successful; otherwise, null.</returns>
    Task<ClientResponseDto?> CreateClientAsync(CreateClientRequest request, CancellationToken ct);

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="countryCode">Optional country code filter.</param>
    /// <param name="riskLevel">Optional risk level filter.</param>
    /// <param name="kycStatus">Optional KYC status filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated response with clients list.</returns>
    Task<PagedClientsResponseDto> GetPagedClientsAsync(
        int pageNumber,
        int pageSize,
        string? countryCode = null,
        string? riskLevel = null,
        string? kycStatus = null,
        CancellationToken ct = default);

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
