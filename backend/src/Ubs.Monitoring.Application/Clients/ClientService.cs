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

    public ClientService(IClientRepository clients, IClientFileImportService fileImport)
    {
        _clients = clients;
        _fileImport = fileImport;
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
    /// The created client data if successful; otherwise, <c>null</c>.
    /// </returns>
    public async Task<ClientResponseDto?> CreateClientAsync(CreateClientRequest request, CancellationToken ct)
    {
        // Validate input
        var (isValid, errorMessage) = ValidateClientRequest(request);
        if (!isValid)
            return null;

        // Create domain entity
        var client = CreateClientFromRequest(request);

        // Persist
        await _clients.AddAsync(client, ct);
        await _clients.SaveChangesAsync(ct);

        // Return DTO
        return MapToResponseDto(client);
    }

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="countryCode">Optional country code filter.</param>
    /// <param name="riskLevel">Optional risk level filter (as string).</param>
    /// <param name="kycStatus">Optional KYC status filter (as string).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Paginated response with clients list.
    /// </returns>
    public async Task<PagedClientsResponseDto> GetPagedClientsAsync(
        int pageNumber,
        int pageSize,
        string? countryCode = null,
        string? riskLevel = null,
        string? kycStatus = null,
        CancellationToken ct = default)
    {
        // Validate pagination parameters
        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
        if (pageSize > 100)
            throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

        // Parse enum filters
        RiskLevel? riskLevelEnum = null;
        if (!string.IsNullOrWhiteSpace(riskLevel) && Enum.TryParse<RiskLevel>(riskLevel, ignoreCase: true, out var parsedRisk))
            riskLevelEnum = parsedRisk;

        KycStatus? kycStatusEnum = null;
        if (!string.IsNullOrWhiteSpace(kycStatus) && Enum.TryParse<KycStatus>(kycStatus, ignoreCase: true, out var parsedKyc))
            kycStatusEnum = parsedKyc;

        // Get data from repository
        var (items, totalCount) = await _clients.GetPagedAsync(
            pageNumber,
            pageSize,
            countryCode,
            riskLevelEnum,
            kycStatusEnum,
            ct
        );

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Map to DTOs
        var clientDtos = items.Select(MapToResponseDto).ToList();

        return new PagedClientsResponseDto(
            Items: clientDtos,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalPages: totalPages
        );
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
        try
        {
            // Parse file (CSV or Excel)
            var rows = _fileImport.ParseFile(fileStream, fileName);

            // Process all rows
            var (successCount, errors) = await ProcessImportRowsAsync(rows, ct);

            // Build result
            return BuildImportResult(rows.Count, successCount, errors);
        }
        catch (Exception ex)
        {
            // File parsing error
            return BuildFileParsingErrorResult(ex.Message);
        }
    }

    /// <summary>
    /// Processes all import rows and creates clients.
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
                var lineNumber = i + 2; // +2 because: index starts at 0, and row 1 is header
                var processResult = await ProcessSingleRowAsync(rows[i], lineNumber, ct);

                if (processResult.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    errors.Add(processResult.Error!);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportErrorDto(
                    LineNumber: i + 2,
                    ClientName: rows[i].Name ?? "Unknown",
                    ErrorMessage: ex.Message
                ));
            }
        }

        // Save all valid clients in a single transaction
        if (successCount > 0)
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
        // Convert row to request
        var request = row.ToRequest();

        // Validate
        var (isValid, errorMessage) = ValidateClientRequest(request);
        if (!isValid)
        {
            return (false, new ImportErrorDto(lineNumber, request.Name ?? "Unknown", errorMessage!));
        }

        // Create client
        var client = CreateClientFromRequest(request);

        // Add to repository (will be saved later in batch)
        await _clients.AddAsync(client, ct);

        return (true, null);
    }

    /// <summary>
    /// Creates a client domain entity from a request.
    /// </summary>
    /// <param name="request">The client creation request.</param>
    /// <returns>A new client domain entity.</returns>
    private static Client CreateClientFromRequest(CreateClientRequest request) =>
        new(
            legalType: request.LegalType,
            name: request.Name.Trim(),
            contactNumber: request.ContactNumber.Trim(),
            addressJson: request.AddressJson,
            countryCode: request.CountryCode.Trim(),
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
    /// Validates a client creation request.
    /// </summary>
    /// <param name="request">The client request to validate.</param>
    /// <returns>
    /// A tuple containing validation result and error message.
    /// IsValid is true if all validations pass; otherwise, false with error message.
    /// </returns>
    private static (bool IsValid, string? ErrorMessage) ValidateClientRequest(CreateClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (false, "Name is required");

        if (string.IsNullOrWhiteSpace(request.ContactNumber))
            return (false, "Contact number is required");

        if (request.AddressJson is null)
            return (false, "Address is required");

        if (string.IsNullOrWhiteSpace(request.CountryCode))
            return (false, "Country code is required");

        return (true, null);
    }

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
