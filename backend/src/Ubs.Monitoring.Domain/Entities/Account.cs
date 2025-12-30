using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }

    public Guid ClientId { get; private set; }
    public Client Client { get; set; } = null!; 

    public string AccountIdentifier { get; private set; } = null!; 
    public string CountryCode { get; private set; } = null!;
    public AccountType AccountType { get; set; }
    public string CurrencyCode { get; private set; } = null!; 
    public AccountStatus Status { get; private set; } 

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<AccountIdentifier> Identifiers { get; set; } = new List<AccountIdentifier>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
