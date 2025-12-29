using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Transaction
{
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

    public Case? Case { get; set; } // EF Core navigation
}
