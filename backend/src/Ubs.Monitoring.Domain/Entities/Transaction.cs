using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Transaction
{
    private Transaction() { }

    public Transaction(
        Guid accountId,
        Guid clientId,
        TransactionType type,
        decimal amount,
        string currencyCode,
        string baseCurrencyCode,
        decimal baseAmount,
        DateTimeOffset occurredAtUtc,
        TransferMethod? transferMethod = null,
        Guid? fxRateId = null,
        string? cpName = null,
        string? cpBank = null,
        string? cpBranch = null,
        string? cpAccount = null,
        IdentifierType? cpIdentifierType = null,
        string? cpIdentifier = null,
        string? cpCountryCode = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));
        if (baseAmount <= 0)
            throw new ArgumentException("Base amount must be positive", nameof(baseAmount));
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required", nameof(currencyCode));
        if (string.IsNullOrWhiteSpace(baseCurrencyCode))
            throw new ArgumentException("Base currency code is required", nameof(baseCurrencyCode));

        Id = Guid.NewGuid();
        AccountId = accountId;
        ClientId = clientId;
        Type = type;
        TransferMethod = transferMethod;
        Amount = amount;
        CurrencyCode = currencyCode.ToUpperInvariant();
        BaseCurrencyCode = baseCurrencyCode.ToUpperInvariant();
        BaseAmount = baseAmount;
        FxRateId = fxRateId;
        OccurredAtUtc = occurredAtUtc;
        CpName = cpName;
        CpBank = cpBank;
        CpBranch = cpBranch;
        CpAccount = cpAccount;
        CpIdentifierType = cpIdentifierType;
        CpIdentifier = cpIdentifier;
        CpCountryCode = cpCountryCode?.ToUpperInvariant();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }
    public Account Account { get; set; } = null!;
    public Guid ClientId { get; private set; }
    public Client Client { get; set; } = null!;

    public TransactionType Type { get; private set; }
    public TransferMethod? TransferMethod { get; private set; }

    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; } = null!;

    public string BaseCurrencyCode { get; private set; } = null!;
    public decimal BaseAmount { get; private set; }

    public Guid? FxRateId { get; private set; }
    public FxRate? FxRate { get; set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? CpName { get; private set; }
    public string? CpBank { get; private set; }
    public string? CpBranch { get; private set; }
    public string? CpAccount { get; private set; }
    public IdentifierType? CpIdentifierType { get; private set; }
    public string? CpIdentifier { get; private set; }
    public string? CpCountryCode { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Case? Case { get; set; }
}
