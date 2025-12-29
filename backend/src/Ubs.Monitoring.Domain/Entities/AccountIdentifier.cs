using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class AccountIdentifier
{
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }
    public Account Account { get; set; } = null!; 

    public IdentifierType IdentifierType { get; private set; }
    public string IdentifierValue { get; private set; } = null!;
    public string? IssuedCountryCode { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
