using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.AccountIdentifiers;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common.FileImport;
using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Accounts;

public sealed class AccountService : IAccountService
{
    private readonly IAccountRepository _accounts;
    private readonly IClientRepository _clients;
    private readonly IFileParser<AccountImportRow> _fileParser;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accounts,
        IClientRepository clients,
        IFileParser<AccountImportRow> fileParser,
        ILogger<AccountService> logger)
    {
        _accounts = accounts;
        _clients = clients;
        _fileParser = fileParser;
        _logger = logger;
    }

    public async Task<(AccountResponseDto? Result, string? ErrorMessage)> CreateAccountAsync(
        Guid clientId,
        CreateAccountRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating new account for client {ClientId}: {AccountIdentifier}",
            clientId, request.AccountIdentifier);

        // Verify client exists
        if (!await _clients.ExistsAsync(clientId, ct))
        {
            _logger.LogWarning("Account creation failed: Client {ClientId} not found", clientId);
            return (null, $"Client with ID '{clientId}' not found.");
        }

        // Check for duplicate account identifier
        if (await _accounts.AccountIdentifierExistsAsync(request.AccountIdentifier, ct))
        {
            _logger.LogWarning("Account creation failed: Account identifier {AccountIdentifier} already exists",
                request.AccountIdentifier);
            return (null, $"Account identifier '{request.AccountIdentifier}' already exists.");
        }

        try
        {
            // Create domain entity (Domain validates invariants)
            var account = new Account(
                clientId: clientId,
                accountIdentifier: request.AccountIdentifier.Trim(),
                countryCode: request.CountryCode.Trim().ToUpperInvariant(),
                accountType: request.AccountType,
                currencyCode: request.CurrencyCode.Trim().ToUpperInvariant()
            );

            // Persist
            _accounts.Add(account);
            await _accounts.SaveChangesAsync(ct);

            _logger.LogInformation("Account created successfully: {AccountId} for client {ClientId}",
                account.Id, clientId);

            return (MapToResponseDto(account), null);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Account creation failed for client {ClientId}: {ErrorMessage}",
                clientId, ex.Message);
            return (null, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all accounts for a specific client.
    /// </summary>
    public async Task<(IReadOnlyList<AccountResponseDto>? Result, string? ErrorMessage)> GetAccountsByClientIdAsync(
        Guid clientId,
        CancellationToken ct)
    {
        // Verify client exists
        if (!await _clients.ExistsAsync(clientId, ct))
        {
            _logger.LogWarning("Get accounts failed: Client {ClientId} not found", clientId);
            return (null, $"Client with ID '{clientId}' not found.");
        }

        var accounts = await _accounts.GetByClientIdAsync(clientId, ct);

        _logger.LogDebug("Retrieved {Count} accounts for client {ClientId}", accounts.Count, clientId);

        return (accounts.Select(MapToResponseDto).ToList(), null);
    }

    /// <summary>
    /// Retrieves detailed information about a specific account.
    /// </summary>
    public async Task<AccountDetailDto?> GetAccountByIdAsync(Guid accountId, CancellationToken ct)
    {
        var account = await _accounts.GetByIdWithDetailsAsync(accountId, ct);
        return account is null ? null : MapToDetailDto(account);
    }

    /// <summary>
    /// Imports multiple accounts for a client from a CSV or Excel file.
    /// </summary>
    public async Task<(AccountImportResultDto? Result, string? ErrorMessage)> ImportAccountsFromFileAsync(
        Guid clientId,
        Stream fileStream,
        string fileName,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting account import from file: {FileName} for client {ClientId}", fileName, clientId);

        if (!await _clients.ExistsAsync(clientId, ct))
        {
            _logger.LogWarning("Account import failed: Client {ClientId} not found", clientId);
            return (null, $"Client with ID '{clientId}' not found.");
        }

        // Tracks account identifiers within this import batch to detect duplicates in the same file
        var batchAccountIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var rows = _fileParser.ParseFile(fileStream, fileName);
            _logger.LogInformation("Parsed {RowCount} rows from file: {FileName}", rows.Count, fileName);

            var (successCount, errors) = await ProcessImportRowsAsync(clientId, rows, ct, batchAccountIdentifiers);

            _logger.LogInformation("Import completed for {FileName}: {SuccessCount} succeeded, {ErrorCount} failed, {TotalRows} total",
                fileName, successCount, errors.Count, rows.Count);

            return (BuildImportResult(rows.Count, successCount, errors), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing import file: {FileName}", fileName);
            return (BuildFileParsingErrorResult(ex.Message), null);
        }
    }

    private async Task<(int SuccessCount, List<AccountImportErrorDto> Errors)> ProcessImportRowsAsync(
        Guid clientId,
        List<AccountImportRow> rows,
        CancellationToken ct,
        HashSet<string> batchAccountIdentifiers)
    {
        var errors = new List<AccountImportErrorDto>();
        var successCount = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            var lineNumber = i + AccountServiceConstants.ImportLineNumberOffset;
            var processResult = await ProcessSingleRowAsync(clientId, rows[i], lineNumber, ct, batchAccountIdentifiers);

            if (processResult.IsSuccess)
            {
                successCount++;

                // Save batch periodically to optimize memory usage
                if (successCount % AccountServiceConstants.ImportBatchSize == 0)
                {
                    await _accounts.SaveChangesAsync(ct);
                    _logger.LogDebug("Saved batch of {BatchSize} accounts. Total processed: {TotalProcessed}",
                        AccountServiceConstants.ImportBatchSize, successCount);
                }
            }
            else
            {
                errors.Add(processResult.Error!);
            }
        }

        // Save remaining records (last partial batch)
        if (successCount % AccountServiceConstants.ImportBatchSize != 0 && successCount > 0)
        {
            await _accounts.SaveChangesAsync(ct);
        }

        return (successCount, errors);
    }

    private async Task<(bool IsSuccess, AccountImportErrorDto? Error)> ProcessSingleRowAsync(
        Guid clientId,
        AccountImportRow row,
        int lineNumber,
        CancellationToken ct,
        HashSet<string> batchAccountIdentifiers)
    {
        try
        {
            var request = row.ToRequest();

            if (await _accounts.AccountIdentifierExistsAsync(request.AccountIdentifier, ct))
            {
                return (false, new AccountImportErrorDto(
                    lineNumber,
                    row.AccountIdentifier ?? "Unknown",
                    $"Account identifier '{request.AccountIdentifier}' already exists."));
            }

            if (batchAccountIdentifiers.Contains(row.AccountIdentifier))
            {
                return (false, new AccountImportErrorDto(
                    lineNumber,
                    row.AccountIdentifier ?? "Unknown",
                    $"Account identifier '{row.AccountIdentifier}' appears multiple times in this import file."
                ));
            }

            var account = new Account(
                clientId: clientId,
                accountIdentifier: request.AccountIdentifier.Trim(),
                countryCode: request.CountryCode.Trim().ToUpperInvariant(),
                accountType: request.AccountType,
                currencyCode: request.CurrencyCode.Trim().ToUpperInvariant()
            );

            _accounts.Add(account);

            batchAccountIdentifiers.Add(row.AccountIdentifier);

            return (true, null);
        }
        catch (ArgumentException ex)
        {
            return (false, new AccountImportErrorDto(lineNumber, row.AccountIdentifier ?? "Unknown", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return (false, new AccountImportErrorDto(lineNumber, row.AccountIdentifier ?? "Unknown", ex.Message));
        }
    }

    private static AccountImportResultDto BuildImportResult(
        int totalProcessed,
        int successCount,
        List<AccountImportErrorDto> errors) =>
        new(
            TotalProcessed: totalProcessed,
            SuccessCount: successCount,
            ErrorCount: errors.Count,
            Errors: errors
        );

    private static AccountImportResultDto BuildFileParsingErrorResult(string errorMessage) =>
        new(
            TotalProcessed: 0,
            SuccessCount: 0,
            ErrorCount: 1,
            Errors: new List<AccountImportErrorDto>
            {
                new(0, "File", $"Error parsing file: {errorMessage}")
            }
        );

    private static AccountResponseDto MapToResponseDto(Account account) =>
        new(
            Id: account.Id,
            ClientId: account.ClientId,
            AccountIdentifier: account.AccountIdentifier,
            CountryCode: account.CountryCode,
            AccountType: account.AccountType,
            CurrencyCode: account.CurrencyCode,
            Status: account.Status,
            CreatedAtUtc: account.CreatedAtUtc,
            UpdatedAtUtc: account.UpdatedAtUtc
        );

    private static AccountDetailDto MapToDetailDto(Account account) =>
        new(
            Id: account.Id,
            ClientId: account.ClientId,
            ClientName: account.Client?.Name,
            AccountIdentifier: account.AccountIdentifier,
            CountryCode: account.CountryCode,
            AccountType: account.AccountType,
            CurrencyCode: account.CurrencyCode,
            Status: account.Status,
            CreatedAtUtc: account.CreatedAtUtc,
            UpdatedAtUtc: account.UpdatedAtUtc,
            Identifiers: account.Identifiers.Select(MapToIdentifierDto).ToList()
        );

    private static AccountIdentifierDto MapToIdentifierDto(AccountIdentifier identifier) =>
        new(
            Id: identifier.Id,
            IdentifierType: identifier.IdentifierType,
            IdentifierValue: identifier.IdentifierValue,
            IssuedCountryCode: identifier.IssuedCountryCode,
            CreatedAtUtc: identifier.CreatedAtUtc
        );
}
