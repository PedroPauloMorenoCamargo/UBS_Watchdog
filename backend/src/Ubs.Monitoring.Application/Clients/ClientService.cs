using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Service implementation for client business operations.
/// </summary>
public sealed class ClientService : IClientService
{
    private readonly IClientRepository _clients;
    private readonly IClientFileImportService _fileImport;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clients,
        IClientFileImportService fileImport,
        ILogger<ClientService> logger)
    {
        _clients = clients;
        _fileImport = fileImport;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new client with validation.
    /// </summary>
    /// <param name="request">
    /// The client creation request data.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A tuple containing the created client data and error message.
    /// If successful, Result contains the client data and ErrorMessage is null.
    /// If failed, Result is null and ErrorMessage contains the validation error from the domain.
    /// </returns>
    public async Task<(ClientResponseDto? Result, string? ErrorMessage)> CreateClientAsync(CreateClientRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating new client: {Name}, LegalType: {LegalType}", request.Name, request.LegalType);

        try
        {
            // Create domain entity (Domain validates invariants)
            var client = CreateClientFromRequest(request);

            // Persist
            _clients.Add(client);
            await _clients.SaveChangesAsync(ct);

            _logger.LogInformation("Client created successfully: {ClientId}, Name: {Name}", client.Id, client.Name);

            // Return DTO
            return (MapToResponseDto(client), null);
        }
        catch (ArgumentException ex)
        {
            // Domain validation failed - return specific error message
            // (includes ArgumentNullException which inherits from ArgumentException)
            _logger.LogWarning("Client creation failed for {Name}: {ErrorMessage}", request.Name, ex.Message);
            return (null, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="query">Query object containing pagination, sorting, and filter parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated result containing client DTOs and metadata.</returns>
    public async Task<PagedResult<ClientResponseDto>> GetPagedClientsAsync(ClientQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogDebug("Fetching clients page {Page} with size {PageSize}, filters: Country={CountryCode}, Risk={RiskLevel}, KYC={KycStatus}",
            query.Page.Page, query.Page.PageSize, query.CountryCode, query.RiskLevel, query.KycStatus);

        var pagedClients = await _clients.GetPagedAsync(query, ct);

        return pagedClients.Map(MapToResponseDto);
    }

    /// <summary>
    /// Retrieves detailed information about a specific client.
    /// </summary>
    /// <param name="clientId">
    /// The unique identifier of the client.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The client details if found; otherwise, <c>null</c>.
    /// </returns>
    public async Task<ClientDetailDto?> GetClientByIdAsync(Guid clientId, CancellationToken ct)
    {
        var client = await _clients.GetByIdWithDetailsAsync(clientId, ct);
        return client is null ? null : MapToDetailDto(client);
    }

    /// <summary>
    /// Imports multiple clients from a CSV or Excel file with error handling.
    /// </summary>
    /// <param name="fileStream">
    /// The file stream containing client data.
    /// </param>
    /// <param name="fileName">
    /// The file name (used to determine format: .csv or .xlsx/.xls).
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Import result with success count and errors.
    /// </returns>
    public async Task<ImportResultDto> ImportClientsFromFileAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        _logger.LogInformation("Starting import from file: {FileName}", fileName);

        try
        {
            // Parse file (CSV or Excel)
            var rows = _fileImport.ParseFile(fileStream, fileName);
            _logger.LogInformation("Parsed {RowCount} rows from file: {FileName}", rows.Count, fileName);

            // Process all rows
            var (successCount, errors) = await ProcessImportRowsAsync(rows, ct);

            _logger.LogInformation("Import completed for {FileName}: {SuccessCount} succeeded, {ErrorCount} failed, {TotalRows} total",
                fileName, successCount, errors.Count, rows.Count);

            // Build result
            return BuildImportResult(rows.Count, successCount, errors);
        }
        catch (Exception ex)
        {
            // File parsing error
            _logger.LogError(ex, "Error parsing import file: {FileName}", fileName);
            return BuildFileParsingErrorResult(ex.Message);
        }
    }

    /// <summary>
    /// Processes all import rows and creates clients.
    /// Uses batch processing to optimize memory usage for large imports.
    /// </summary>
    /// <param name="rows">List of rows to process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple containing success count and list of errors.</returns>
    private async Task<(int SuccessCount, List<ImportErrorDto> Errors)> ProcessImportRowsAsync(
        List<ClientImportRow> rows,
        CancellationToken ct)
    {
        var errors = new List<ImportErrorDto>();
        var successCount = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            try
            {
                var lineNumber = i + ClientServiceConstants.ImportLineNumberOffset;
                var processResult = await ProcessSingleRowAsync(rows[i], lineNumber, ct);

                if (processResult.IsSuccess)
                {
                    successCount++;

                    // Save batch periodically to optimize memory usage
                    if (successCount % ClientServiceConstants.ImportBatchSize == 0)
                    {
                        await _clients.SaveChangesAsync(ct);
                        _logger.LogDebug("Saved batch of {BatchSize} clients. Total processed: {TotalProcessed}",
                            ClientServiceConstants.ImportBatchSize, successCount);
                    }
                }
                else
                {
                    errors.Add(processResult.Error!);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(
                    LineNumber: i + ClientServiceConstants.ImportLineNumberOffset,
                    ClientName: rows[i].Name ?? "Unknown",
                    ErrorMessage: ex.Message
                ));
            }
        }

        // Save remaining records (last partial batch)
        if (successCount % ClientServiceConstants.ImportBatchSize != 0 && successCount > 0)
        {
            await _clients.SaveChangesAsync(ct);
        }

        return (successCount, errors);
    }

    /// <summary>
    /// Processes a single import row and creates a client.
    /// </summary>
    /// <param name="row">The row to process.</param>
    /// <param name="lineNumber">The line number for error reporting.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Processing result indicating success or error.</returns>
    private async Task<(bool IsSuccess, ImportErrorDto? Error)> ProcessSingleRowAsync(
        ClientImportRow row,
        int lineNumber,
        CancellationToken ct)
    {
        try
        {
            // Convert row to request
            var request = row.ToRequest();

            // Create client (Domain validates invariants)
            var client = CreateClientFromRequest(request);

            // Add to repository (will be saved later in batch)
            _clients.Add(client);

            return (true, null);
        }
        catch (ArgumentException ex)
        {
            // Domain validation failed
            // (includes ArgumentNullException which inherits from ArgumentException)
            return (false, new ImportErrorDto(lineNumber, row.Name ?? "Unknown", ex.Message));
        }
    }

    /// <summary>
    /// Creates a client domain entity from a request.
    /// Normalizes countryCode to uppercase for consistency with repository filters.
    /// </summary>
    /// <param name="request">The client creation request.</param>
    /// <returns>A new client domain entity.</returns>
    private static Client CreateClientFromRequest(CreateClientRequest request) =>
        new(
            legalType: request.LegalType,
            name: request.Name.Trim(),
            contactNumber: request.ContactNumber.Trim(),
            addressJson: request.AddressJson,
            countryCode: request.CountryCode.Trim().ToUpperInvariant(),
            initialRiskLevel: request.InitialRiskLevel
        );

    /// <summary>
    /// Builds the import result DTO.
    /// </summary>
    /// <param name="totalProcessed">Total number of rows processed.</param>
    /// <param name="successCount">Number of successful imports.</param>
    /// <param name="errors">List of errors encountered.</param>
    /// <returns>Import result DTO.</returns>
    private static ImportResultDto BuildImportResult(
        int totalProcessed,
        int successCount,
        List<ImportErrorDto> errors) =>
        new(
            TotalProcessed: totalProcessed,
            SuccessCount: successCount,
            ErrorCount: errors.Count,
            Errors: errors
        );

    /// <summary>
    /// Builds an import result for file parsing errors.
    /// </summary>
    /// <param name="errorMessage">The error message from the parsing exception.</param>
    /// <returns>Import result DTO with parsing error.</returns>
    private static ImportResultDto BuildFileParsingErrorResult(string errorMessage) =>
        new(
            TotalProcessed: 0,
            SuccessCount: 0,
            ErrorCount: 1,
            Errors: new List<ImportErrorDto>
            {
                new ImportErrorDto(0, "File", $"Error parsing file: {errorMessage}")
            }
        );

    /// <summary>
    /// Maps a domain <see cref="Client"/> entity to a <see cref="ClientResponseDto"/>.
    /// </summary>
    /// <param name="client">
    /// The client domain entity.
    /// </param>
    /// <returns>
    /// A data transfer object representing basic client information.
    /// </returns>
    private static ClientResponseDto MapToResponseDto(Client client) =>
        new(
            Id: client.Id,
            LegalType: client.LegalType,
            Name: client.Name,
            ContactNumber: client.ContactNumber,
            AddressJson: client.AddressJson,
            CountryCode: client.CountryCode,
            RiskLevel: client.RiskLevel,
            KycStatus: client.KycStatus,
            CreatedAtUtc: client.CreatedAtUtc,
            UpdatedAtUtc: client.UpdatedAtUtc
        );

    /// <summary>
    /// Maps a domain <see cref="Client"/> entity with related data to a <see cref="ClientDetailDto"/>.
    /// </summary>
    /// <param name="client">
    /// The client domain entity with related entities loaded.
    /// </param>
    /// <returns>
    /// A data transfer object representing detailed client information.
    /// </returns>
    private static ClientDetailDto MapToDetailDto(Client client) =>
        new(
            Id: client.Id,
            LegalType: client.LegalType,
            Name: client.Name,
            ContactNumber: client.ContactNumber,
            AddressJson: client.AddressJson,
            CountryCode: client.CountryCode,
            RiskLevel: client.RiskLevel,
            KycStatus: client.KycStatus,
            CreatedAtUtc: client.CreatedAtUtc,
            UpdatedAtUtc: client.UpdatedAtUtc,
            TotalAccounts: client.Accounts?.Count ?? 0,
            TotalTransactions: client.Transactions?.Count ?? 0,
            TotalCases: client.Cases?.Count ?? 0
        );
}
