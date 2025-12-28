using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public LegalType LegalType { get; set; }
    public string Name { get; set; } = null!;
    public string ContactNumber { get; set; } = null!;
    public JsonDocument AddressJson { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public RiskLevel RiskLevel { get; set; }
    public KycStatus KycStatus { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
