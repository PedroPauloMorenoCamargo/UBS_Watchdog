using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public TransactionType Type { get; set; }
    public TransferMethod? TransferMethod { get; set; }

    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = null!;

    public string BaseCurrencyCode { get; set; } = null!;
    public decimal BaseAmount { get; set; }

    public Guid? FxRateId { get; set; }
    public FxRate? FxRate { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    public string? CpName { get; set; }
    public string? CpBank { get; set; }
    public string? CpBranch { get; set; }
    public string? CpAccount { get; set; }
    public IdentifierType? CpIdentifierType { get; set; }
    public string? CpIdentifier { get; set; }
    public string? CpCountryCode { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Case? Case { get; set; }
}
