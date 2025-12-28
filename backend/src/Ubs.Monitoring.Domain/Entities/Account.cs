using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public string AccountIdentifier { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public AccountType AccountType { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public AccountStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<AccountIdentifier> Identifiers { get; set; } = new List<AccountIdentifier>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
