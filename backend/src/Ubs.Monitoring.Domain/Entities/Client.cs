using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; }
    public LegalType LegalType { get; private set; } 
    public string Name { get; set; } = null!;
    public string ContactNumber { get; set; } = null!;
    public JsonDocument AddressJson { get; set; } = null!;
    public string CountryCode { get; private set; } = null!; 
    public RiskLevel RiskLevel { get; private set; } 
    public KycStatus KycStatus { get; private set; } 
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
