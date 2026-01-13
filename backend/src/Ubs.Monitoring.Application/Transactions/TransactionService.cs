using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Accounts;
using Ubs.Monitoring.Application.Common.FileImport;
using Ubs.Monitoring.Application.FxRates;
using Ubs.Monitoring.Application.Transactions.Repositories;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Application.Transactions.Compliance;


namespace Ubs.Monitoring.Application.Transactions;

public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactions;
    private readonly IAccountRepository _accounts;
    private readonly IFxRateService _fxRateService;
    private readonly IFileParser<TransactionImportRow> _fileParser;
    private readonly ILogger<TransactionService> _logger;
    private readonly ITransactionComplianceChecker _compliance;


    public TransactionService(
        ITransactionRepository transactions,
        IAccountRepository accounts,
        IFxRateService fxRateService,
        IFileParser<TransactionImportRow> fileParser,
        ILogger<TransactionService> logger,
        ITransactionComplianceChecker compliance)
    {
        _transactions = transactions;
        _accounts = accounts;
        _fxRateService = fxRateService;
        _fileParser = fileParser;
        _logger = logger;
        _compliance = compliance;
    }

    public async Task<(TransactionResponseDto? Result, string? ErrorMessage)> CreateTransactionAsync(
        CreateTransactionRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating new transaction for account {AccountId}: {Type} {Amount} {Currency}",
            request.AccountId, request.Type, request.Amount, request.CurrencyCode);

        // Verify account exists and get client info
        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
        {
            _logger.LogWarning("Transaction creation failed: Account {AccountId} not found", request.AccountId);
            return (null, $"Account with ID '{request.AccountId}' not found.");
        }

        var (baseAmount, fxRateId, fxError) = await _fxRateService.ConvertToBaseCurrencyAsync(
            request.Amount, request.CurrencyCode, ct);

        if (fxError is not null)
        {
            _logger.LogWarning("Transaction creation failed: {Error}", fxError);
            return (null, fxError);
        }

        try
        {
            var transaction = new Transaction(
                accountId: request.AccountId,
                clientId: account.ClientId,
                type: request.Type,
                amount: request.Amount,
                currencyCode: request.CurrencyCode,
                baseCurrencyCode: _fxRateService.BaseCurrencyCode,
                baseAmount: baseAmount,
                occurredAtUtc: request.OccurredAtUtc,
                transferMethod: request.TransferMethod,
                fxRateId: fxRateId,
                cpName: request.CpName,
                cpBank: request.CpBank,
                cpBranch: request.CpBranch,
                cpAccount: request.CpAccount,
                cpIdentifierType: request.CpIdentifierType,
                cpIdentifier: request.CpIdentifier,
                cpCountryCode: request.CpCountryCode
            );

            // Persist
            _transactions.Add(transaction);
            await _transactions.SaveChangesAsync(ct);

            _logger.LogInformation("Transaction created successfully: {TransactionId} for account {AccountId}",
                transaction.Id, request.AccountId);

            // Check compliance and automatically create case if violations found
            await _compliance.CheckAndCreateCaseIfNeededAsync(transaction, ct);

            return (MapToResponseDto(transaction), null);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Transaction creation failed for account {AccountId}: {ErrorMessage}",
                request.AccountId, ex.Message);
            return (null, ex.Message);
        }
    }

    public async Task<TransactionDetailDto?> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct)
    {
        var transaction = await _transactions.GetByIdWithDetailsAsync(transactionId, ct);
        return transaction is null ? null : MapToDetailDto(transaction);
    }

    public async Task<PagedTransactionsResponseDto> GetTransactionsAsync(
        TransactionFilterRequest filter,
        CancellationToken ct)
    {
        _logger.LogDebug("Retrieving transactions with filter: {@Filter}", filter);

        var (items, totalCount) = await _transactions.GetPagedAsync(filter, ct);

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedTransactionsResponseDto(
            Items: items.Select(MapToResponseDto).ToList(),
            TotalCount: totalCount,
            PageNumber: filter.Page,
            PageSize: filter.PageSize,
            TotalPages: totalPages
        );
    }

    public async Task<(TransactionImportResultDto? Result, string? ErrorMessage)> ImportTransactionsFromFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting transaction import from file: {FileName}", fileName);

        // Cache de contas resolvidas para evitar queries repetidas no mesmo batch de import
        var accountCache = new Dictionary<string, Account>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var rows = _fileParser.ParseFile(fileStream, fileName);
            _logger.LogInformation("Parsed {RowCount} rows from file: {FileName}", rows.Count, fileName);

            var (successCount, errors) = await ProcessImportRowsAsync(rows, ct, accountCache);

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

    #region Private Methods - Import Processing

    private async Task<(int SuccessCount, List<TransactionImportErrorDto> Errors)> ProcessImportRowsAsync(
        List<TransactionImportRow> rows,
        CancellationToken ct,
        Dictionary<string, Account> accountCache)
    {
        var errors = new List<TransactionImportErrorDto>();
        var successCount = 0;
        var lastSaveCount = 0;
        var transactionsToCheck = new List<Transaction>();

        for (int i = 0; i < rows.Count; i++)
        {
            var lineNumber = i + TransactionServiceConstants.ImportLineNumberOffset;
            var processResult = await ProcessSingleRowAsync(rows[i], lineNumber, ct, accountCache);

            if (processResult.IsSuccess)
            {
                successCount++;
                if (processResult.Transaction is not null)
                {
                    transactionsToCheck.Add(processResult.Transaction);
                }

                // Save batch periodically
                if (successCount % TransactionServiceConstants.ImportBatchSize == 0)
                {
                    await _transactions.SaveChangesAsync(ct);

                    // Check compliance for saved transactions
                    await CheckComplianceForBatchAsync(transactionsToCheck, ct);
                    transactionsToCheck.Clear();

                    lastSaveCount = successCount;
                    _logger.LogDebug("Saved batch of {BatchSize} transactions. Total processed: {TotalProcessed}",
                        TransactionServiceConstants.ImportBatchSize, successCount);
                }
            }
            else
            {
                errors.Add(processResult.Error!);
            }
        }

        // Save remaining records only if there were changes since last save
        if (successCount > lastSaveCount)
        {
            await _transactions.SaveChangesAsync(ct);

            // Check compliance for remaining transactions
            if (transactionsToCheck.Count > 0)
            {
                await CheckComplianceForBatchAsync(transactionsToCheck, ct);
            }

            _logger.LogDebug("Saved final batch of {Count} remaining transactions", successCount - lastSaveCount);
        }

        return (successCount, errors);
    }

    private async Task CheckComplianceForBatchAsync(List<Transaction> transactions, CancellationToken ct)
    {
        foreach (var transaction in transactions)
        {
            try
            {
                await _compliance.CheckAndCreateCaseIfNeededAsync(transaction, ct);
            }
            catch (Exception ex)
            {
                // Log but don't fail the import
                _logger.LogError(ex, "Failed to check compliance for transaction {TransactionId} during import", transaction.Id);
            }
        }
    }

    private async Task<(bool IsSuccess, TransactionImportErrorDto? Error, Transaction? Transaction)> ProcessSingleRowAsync(
        TransactionImportRow row,
        int lineNumber,
        CancellationToken ct,
        Dictionary<string, Account> accountCache)
    {
        try
        {
            if (!accountCache.TryGetValue(row.AccountIdentifier, out var account))
            {
                account = await _accounts.GetByAccountIdentifierAsync(row.AccountIdentifier, ct);
                if (account is null)
                {
                    return (false, new TransactionImportErrorDto(
                        lineNumber,
                        row.AccountIdentifier,
                        $"Account with identifier '{row.AccountIdentifier}' not found."), null);
                }
                accountCache[row.AccountIdentifier] = account;
            }

            var request = row.ToRequest(account.Id);

            // Validate transfer fields
            var transferValidationError = ValidateTransferFieldsForImport(request, lineNumber, row.AccountIdentifier);
            if (transferValidationError is not null)
            {
                return (false, transferValidationError, null);
            }

            // Validate Brazilian transfer counterparty exists in system
            if (request.Type == TransactionType.Transfer &&
                request.CpCountryCode == "BR" &&
                request.CpIdentifierType.HasValue &&
                !string.IsNullOrWhiteSpace(request.CpIdentifier))
            {
                var cpExists = await _accounts.IdentifierExistsGloballyAsync(
                    request.CpIdentifierType.Value, request.CpIdentifier.Trim(), ct);

                if (!cpExists)
                {
                    return (false, new TransactionImportErrorDto(
                        lineNumber,
                        row.AccountIdentifier,
                        $"Counterparty identifier '{request.CpIdentifier}' of type '{request.CpIdentifierType}' was not found in the system. " +
                        $"For Brazilian transfers (BR), the counterparty must have a registered account with this identifier."), null);
                }
            }

            // Calculate base amount
            var (baseAmount, fxRateId, fxError) = await _fxRateService.ConvertToBaseCurrencyAsync(
                request.Amount, request.CurrencyCode, ct);

            if (fxError is not null)
            {
                return (false, new TransactionImportErrorDto(lineNumber, row.AccountIdentifier, fxError), null);
            }

            // Create transaction
            var transaction = new Transaction(
                accountId: account.Id,
                clientId: account.ClientId,
                type: request.Type,
                amount: request.Amount,
                currencyCode: request.CurrencyCode,
                baseCurrencyCode: _fxRateService.BaseCurrencyCode,
                baseAmount: baseAmount,
                occurredAtUtc: request.OccurredAtUtc,
                transferMethod: request.TransferMethod,
                fxRateId: fxRateId,
                cpName: request.CpName,
                cpBank: request.CpBank,
                cpBranch: request.CpBranch,
                cpAccount: request.CpAccount,
                cpIdentifierType: request.CpIdentifierType,
                cpIdentifier: request.CpIdentifier,
                cpCountryCode: request.CpCountryCode
            );

            _transactions.Add(transaction);
            return (true, null, transaction);
        }
        catch (ArgumentException ex)
        {
            return (false, new TransactionImportErrorDto(lineNumber, row.AccountIdentifier, ex.Message), null);
        }
        catch (InvalidOperationException ex)
        {
            return (false, new TransactionImportErrorDto(lineNumber, row.AccountIdentifier, ex.Message), null);
        }
    }

    private static TransactionImportErrorDto? ValidateTransferFieldsForImport(
        CreateTransactionRequest request,
        int lineNumber,
        string accountIdentifier)
    {
        if (request.Type != TransactionType.Transfer)
            return null;

        if (request.TransferMethod is null)
            return new TransactionImportErrorDto(lineNumber, accountIdentifier, "TransferMethod is required for Transfer transactions.");

        if (string.IsNullOrWhiteSpace(request.CpCountryCode))
            return new TransactionImportErrorDto(lineNumber, accountIdentifier, "CpCountryCode is required for Transfer transactions.");

        if (request.CpIdentifierType is null)
            return new TransactionImportErrorDto(lineNumber, accountIdentifier, "CpIdentifierType is required for Transfer transactions.");

        if (string.IsNullOrWhiteSpace(request.CpIdentifier))
            return new TransactionImportErrorDto(lineNumber, accountIdentifier, "CpIdentifier is required for Transfer transactions.");

        return null;
    }

    #endregion

    #region Private Methods - Result Builders

    private static TransactionImportResultDto BuildImportResult(
        int totalProcessed,
        int successCount,
        List<TransactionImportErrorDto> errors) =>
        new(
            TotalProcessed: totalProcessed,
            SuccessCount: successCount,
            ErrorCount: errors.Count,
            Errors: errors
        );

    private static TransactionImportResultDto BuildFileParsingErrorResult(string errorMessage) =>
        new(
            TotalProcessed: 0,
            SuccessCount: 0,
            ErrorCount: 1,
            Errors: new List<TransactionImportErrorDto>
            {
                new(0, "File", $"Error parsing file: {errorMessage}")
            }
        );

    #endregion

    #region Private Methods - Mapping

    private static TransactionResponseDto MapToResponseDto(Transaction transaction) =>
        new(
            Id: transaction.Id,
            AccountId: transaction.AccountId,
            ClientId: transaction.ClientId,
            Type: transaction.Type,
            TransferMethod: transaction.TransferMethod,
            Amount: transaction.Amount,
            CurrencyCode: transaction.CurrencyCode,
            BaseCurrencyCode: transaction.BaseCurrencyCode,
            BaseAmount: transaction.BaseAmount,
            FxRateId: transaction.FxRateId,
            OccurredAtUtc: transaction.OccurredAtUtc,
            CpName: transaction.CpName,
            CpBank: transaction.CpBank,
            CpBranch: transaction.CpBranch,
            CpAccount: transaction.CpAccount,
            CpIdentifierType: transaction.CpIdentifierType,
            CpIdentifier: transaction.CpIdentifier,
            CpCountryCode: transaction.CpCountryCode,
            CreatedAtUtc: transaction.CreatedAtUtc
        );

    private static TransactionDetailDto MapToDetailDto(Transaction transaction) =>
        new(
            Id: transaction.Id,
            AccountId: transaction.AccountId,
            AccountIdentifier: transaction.Account?.AccountIdentifier,
            ClientId: transaction.ClientId,
            ClientName: transaction.Client?.Name,
            Type: transaction.Type,
            TransferMethod: transaction.TransferMethod,
            Amount: transaction.Amount,
            CurrencyCode: transaction.CurrencyCode,
            BaseCurrencyCode: transaction.BaseCurrencyCode,
            BaseAmount: transaction.BaseAmount,
            FxRateId: transaction.FxRateId,
            FxRate: transaction.FxRate?.Rate,
            OccurredAtUtc: transaction.OccurredAtUtc,
            CpName: transaction.CpName,
            CpBank: transaction.CpBank,
            CpBranch: transaction.CpBranch,
            CpAccount: transaction.CpAccount,
            CpIdentifierType: transaction.CpIdentifierType,
            CpIdentifier: transaction.CpIdentifier,
            CpCountryCode: transaction.CpCountryCode,
            CreatedAtUtc: transaction.CreatedAtUtc
        );

    #endregion
}
