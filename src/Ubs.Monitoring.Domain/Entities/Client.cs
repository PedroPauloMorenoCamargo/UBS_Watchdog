using System.Text.Json;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Client
{
    private Client() { }

    public Client(
        LegalType legalType,
        string name,
        string contactNumber,
        JsonDocument addressJson,
        string countryCode,
        RiskLevel? initialRiskLevel = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Client name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(contactNumber))
            throw new ArgumentException("Contact number is required", nameof(contactNumber));
        if (addressJson == null)
            throw new ArgumentNullException(nameof(addressJson));
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required", nameof(countryCode));

        Id = Guid.NewGuid();
        LegalType = legalType;
        Name = name;
        ContactNumber = contactNumber;
        AddressJson = addressJson;
        CountryCode = countryCode.ToUpperInvariant();
        RiskLevel = initialRiskLevel ?? Enums.RiskLevel.Low;
        KycStatus = KycStatus.Pending;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public LegalType LegalType { get; private set; }
    public string Name { get; private set; } = null!;
    public string ContactNumber { get; private set; } = null!;
    public JsonDocument AddressJson { get; private set; } = null!;
    public string CountryCode { get; private set; } = null!;
    public RiskLevel RiskLevel { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();

    public void VerifyKyc()
    {
        if (KycStatus != KycStatus.Pending)
            throw new InvalidOperationException($"Cannot verify KYC when status is {KycStatus}. Expected Pending.");

        KycStatus = KycStatus.Verified;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RejectKyc()
    {
        if (KycStatus != KycStatus.Pending)
            throw new InvalidOperationException($"Cannot reject KYC when status is {KycStatus}. Expected Pending.");

        KycStatus = KycStatus.Rejected;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ExpireKyc()
    {
        if (KycStatus != KycStatus.Verified)
            throw new InvalidOperationException($"Cannot expire KYC when status is {KycStatus}. Expected Verified.");

        KycStatus = KycStatus.Expired;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RenewKyc()
    {
        if (KycStatus != KycStatus.Expired && KycStatus != KycStatus.Rejected)
            throw new InvalidOperationException($"Cannot renew KYC when status is {KycStatus}. Expected Expired or Rejected.");

        KycStatus = KycStatus.Pending;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateRiskLevel(RiskLevel newRiskLevel)
    {
        RiskLevel = newRiskLevel;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateContactInfo(string contactNumber, JsonDocument? newAddress = null)
    {
        if (string.IsNullOrWhiteSpace(contactNumber))
            throw new ArgumentException("Contact number is required", nameof(contactNumber));

        ContactNumber = contactNumber;
        if (newAddress != null)
            AddressJson = newAddress;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
