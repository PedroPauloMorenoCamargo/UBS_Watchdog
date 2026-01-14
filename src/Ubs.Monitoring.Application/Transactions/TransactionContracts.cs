using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Transactions;

#region Request DTOs

/// <summary>
/// Request DTO for creating a new transaction.
/// </summary>
public sealed record CreateTransactionRequest(
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string CurrencyCode,
    DateTimeOffset OccurredAtUtc,
    TransferMethod? TransferMethod = null,
    string? CpName = null,
    string? CpBank = null,
    string? CpBranch = null,
    string? CpAccount = null,
    IdentifierType? CpIdentifierType = null,
    string? CpIdentifier = null,
    string? CpCountryCode = null
);

/// <summary>
/// Request DTO for filtering and paginating transactions.
/// </summary>
public sealed record TransactionFilterRequest(
    Guid? ClientId = null,
    Guid? AccountId = null,
    TransactionType? Type = null,
    TransferMethod? TransferMethod = null,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? CurrencyCode = null,
    string? CpCountryCode = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    bool SortDescending = true
);

#endregion

#region Response DTOs

/// <summary>
/// Response DTO with basic transaction information.
/// </summary>
public sealed record TransactionResponseDto(
    Guid Id,
    Guid AccountId,
    Guid ClientId,
    TransactionType Type,
    TransferMethod? TransferMethod,
    decimal Amount,
    string CurrencyCode,
    string BaseCurrencyCode,
    decimal BaseAmount,
    Guid? FxRateId,
    DateTimeOffset OccurredAtUtc,
    string? CpName,
    string? CpBank,
    string? CpBranch,
    string? CpAccount,
    IdentifierType? CpIdentifierType,
    string? CpIdentifier,
    string? CpCountryCode,
    DateTimeOffset CreatedAtUtc
);

/// <summary>
/// Response DTO with detailed transaction information including related entities.
/// </summary>
public sealed record TransactionDetailDto(
    Guid Id,
    Guid AccountId,
    string? AccountIdentifier,
    Guid ClientId,
    string? ClientName,
    TransactionType Type,
    TransferMethod? TransferMethod,
    decimal Amount,
    string CurrencyCode,
    string BaseCurrencyCode,
    decimal BaseAmount,
    Guid? FxRateId,
    decimal? FxRate,
    DateTimeOffset OccurredAtUtc,
    string? CpName,
    string? CpBank,
    string? CpBranch,
    string? CpAccount,
    IdentifierType? CpIdentifierType,
    string? CpIdentifier,
    string? CpCountryCode,
    DateTimeOffset CreatedAtUtc
);

/// <summary>
/// Response DTO for paginated list of transactions.
/// </summary>
public sealed record PagedTransactionsResponseDto(
    IReadOnlyList<TransactionResponseDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

#endregion

#region Import DTOs

/// <summary>
/// Response DTO for transaction import operations.
/// </summary>
public sealed record TransactionImportResultDto(
    int TotalProcessed,
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<TransactionImportErrorDto> Errors
);

/// <summary>
/// Details of a transaction import error.
/// </summary>
public sealed record TransactionImportErrorDto(
    int LineNumber,
    string Identifier,
    string ErrorMessage
);

/// <summary>
/// Model for a single row in the transaction import file.
/// </summary>
public sealed class TransactionImportRow
{
    public string AccountIdentifier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? TransferMethod { get; set; }
    public string Amount { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string OccurredAtUtc { get; set; } = string.Empty;
    public string? CpName { get; set; }
    public string? CpBank { get; set; }
    public string? CpBranch { get; set; }
    public string? CpAccount { get; set; }
    public string? CpIdentifierType { get; set; }
    public string? CpIdentifier { get; set; }
    public string? CpCountryCode { get; set; }

    /// <summary>
    /// Converts the import row to a CreateTransactionRequest.
    /// </summary>
    /// <param name="accountId">The resolved account ID from the AccountIdentifier.</param>
    /// <returns>A CreateTransactionRequest DTO.</returns>
    /// <exception cref="InvalidOperationException">Thrown when enum parsing fails.</exception>
    public CreateTransactionRequest ToRequest(Guid accountId)
    {
        if (!Enum.TryParse<TransactionType>(Type, ignoreCase: true, out var transactionType))
            throw new InvalidOperationException($"Invalid transaction type: '{Type}'. Must be 'Deposit', 'Withdrawal', or 'Transfer'.");

        TransferMethod? transferMethod = null;
        if (!string.IsNullOrWhiteSpace(TransferMethod))
        {
            if (!Enum.TryParse<TransferMethod>(TransferMethod, ignoreCase: true, out var parsedMethod))
                throw new InvalidOperationException($"Invalid transfer method: '{TransferMethod}'. Must be 'PIX', 'TED', or 'WIRE'.");
            transferMethod = parsedMethod;
        }

        if (!decimal.TryParse(Amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amount))
            throw new InvalidOperationException($"Invalid amount: '{Amount}'. Must be a valid decimal number.");

        if (!DateTimeOffset.TryParse(OccurredAtUtc, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var occurredAt))
            throw new InvalidOperationException($"Invalid date: '{OccurredAtUtc}'. Must be a valid ISO 8601 date.");

        IdentifierType? cpIdentifierType = null;
        if (!string.IsNullOrWhiteSpace(CpIdentifierType))
        {
            if (!Enum.TryParse<IdentifierType>(CpIdentifierType, ignoreCase: true, out var parsedIdentifierType))
                throw new InvalidOperationException($"Invalid counterparty identifier type: '{CpIdentifierType}'.");
            cpIdentifierType = parsedIdentifierType;
        }

        return new CreateTransactionRequest(
            AccountId: accountId,
            Type: transactionType,
            Amount: amount,
            CurrencyCode: CurrencyCode.Trim().ToUpperInvariant(),
            OccurredAtUtc: occurredAt,
            TransferMethod: transferMethod,
            CpName: string.IsNullOrWhiteSpace(CpName) ? null : CpName.Trim(),
            CpBank: string.IsNullOrWhiteSpace(CpBank) ? null : CpBank.Trim(),
            CpBranch: string.IsNullOrWhiteSpace(CpBranch) ? null : CpBranch.Trim(),
            CpAccount: string.IsNullOrWhiteSpace(CpAccount) ? null : CpAccount.Trim(),
            CpIdentifierType: cpIdentifierType,
            CpIdentifier: string.IsNullOrWhiteSpace(CpIdentifier) ? null : CpIdentifier.Trim(),
            CpCountryCode: string.IsNullOrWhiteSpace(CpCountryCode) ? null : CpCountryCode.Trim().ToUpperInvariant()
        );
    }
}

#endregion
