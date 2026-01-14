using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class AccountIdentifier
{
    private AccountIdentifier() { }

    public AccountIdentifier(
        Guid accountId,
        IdentifierType identifierType,
        string identifierValue,
        string? issuedCountryCode = null)
    {
        if (string.IsNullOrWhiteSpace(identifierValue))
            throw new ArgumentException("Identifier value is required", nameof(identifierValue));

        Id = Guid.NewGuid();
        AccountId = accountId;
        IdentifierType = identifierType;
        IdentifierValue = identifierValue;
        IssuedCountryCode = issuedCountryCode?.ToUpperInvariant();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }
    public Account Account { get; set; } = null!; 

    public IdentifierType IdentifierType { get; private set; }
    public string IdentifierValue { get; private set; } = null!;
    public string? IssuedCountryCode { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
