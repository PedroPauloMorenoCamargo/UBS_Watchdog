using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Domain.Entities;

public class AccountIdentifier
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public IdentifierType IdentifierType { get; set; }
    public string IdentifierValue { get; set; } = null!;
    public string? IssuedCountryCode { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
