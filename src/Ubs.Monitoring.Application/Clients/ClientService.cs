using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Common.FileImport;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Clients;

public sealed class ClientService : IClientService
{
    private readonly IClientRepository _clients;
    private readonly IFileParser<ClientImportRow> _fileParser;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clients,
        IFileParser<ClientImportRow> fileParser,
        ILogger<ClientService> logger)
    {
        _clients = clients;
        _fileParser = fileParser;
        _logger = logger;
    }

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
            _logger.LogWarning("Client creation failed for {Name}: {ErrorMessage}", request.Name, ex.Message);
            return (null, ex.Message);
        }
    }

    public async Task<PagedResult<ClientResponseDto>> GetPagedClientsAsync(ClientQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        _logger.LogDebug("Fetching clients page {Page} with size {PageSize}, filters: Country={CountryCode}, Risk={RiskLevel}, KYC={KycStatus}",
            query.Page.Page, query.Page.PageSize, query.CountryCode, query.RiskLevel, query.KycStatus);

        var pagedClients = await _clients.GetPagedAsync(query, ct);

        return pagedClients.Map(MapToResponseDto);
    }

    public async Task<ClientDetailDto?> GetClientByIdAsync(Guid clientId, CancellationToken ct)
    {
        var client = await _clients.GetByIdWithDetailsAsync(clientId, ct);
        return client is null ? null : MapToDetailDto(client);
    }

    public async Task<ImportResultDto> ImportClientsFromFileAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        _logger.LogInformation("Starting import from file: {FileName}", fileName);

        try
        {
            // Parse file (CSV or Excel)
            var rows = _fileParser.ParseFile(fileStream, fileName);
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
    /// Processes import rows in batches to optimize memory for large imports.
    /// </summary>
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

    private async Task<(bool IsSuccess, ImportErrorDto? Error)> ProcessSingleRowAsync(
        ClientImportRow row,
        int lineNumber,
        CancellationToken ct)
    {
        try
        {
            var request = row.ToRequest();
            var client = CreateClientFromRequest(request);
            _clients.Add(client);

            return (true, null);
        }
        catch (ArgumentException ex)
        {
            return (false, new ImportErrorDto(lineNumber, row.Name ?? "Unknown", ex.Message));
        }
    }

    private static Client CreateClientFromRequest(CreateClientRequest request) =>
        new(
            legalType: request.LegalType,
            name: request.Name.Trim(),
            contactNumber: request.ContactNumber.Trim(),
            addressJson: request.AddressJson,
            countryCode: request.CountryCode.Trim().ToUpperInvariant(),
            initialRiskLevel: request.InitialRiskLevel
        );

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
