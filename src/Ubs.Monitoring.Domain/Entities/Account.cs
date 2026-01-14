using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class Account
{
    private Account()
    {
    }

    public Account(
        Guid clientId,
        string accountIdentifier,
        string countryCode,
        AccountType accountType,
        string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(accountIdentifier))
            throw new ArgumentException("Account identifier is required", nameof(accountIdentifier));
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code is required", nameof(countryCode));
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code is required", nameof(currencyCode));

        Id = Guid.NewGuid();
        ClientId = clientId;
        AccountIdentifier = accountIdentifier;
        CountryCode = countryCode.ToUpperInvariant();
        AccountType = accountType;
        CurrencyCode = currencyCode.ToUpperInvariant();
        Status = AccountStatus.Active;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get;  set; }

    public Guid ClientId { get; private set; }
    public Client Client { get; set; } = null!;

    public string AccountIdentifier { get; set; } = null!;
    public string CountryCode { get; private set; } = null!;
    public AccountType AccountType { get; private set; }
    public string CurrencyCode { get; private set; } = null!;
    public AccountStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    private readonly List<AccountIdentifier> _identifiers = new();
    public IReadOnlyCollection<AccountIdentifier> Identifiers => _identifiers.AsReadOnly();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Case> Cases { get; set; } = new List<Case>();

    public AccountIdentifier AddIdentifier(IdentifierType type, string value, string? issuedCountryCode = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Identifier value is required", nameof(value));
        if (_identifiers.Any(i => i.IdentifierType == type && i.IdentifierValue == value))
            throw new InvalidOperationException($"Identifier {type} with value '{value}' already exists");

        var identifier = new AccountIdentifier(Id, type, value, issuedCountryCode);
        _identifiers.Add(identifier);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return identifier;
    }

    public void Block()
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidOperationException("Cannot block a closed account");
        Status = AccountStatus.Blocked;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidOperationException("Cannot reactivate a closed account");
        Status = AccountStatus.Active;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidOperationException("Account is already closed");
        Status = AccountStatus.Closed;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
