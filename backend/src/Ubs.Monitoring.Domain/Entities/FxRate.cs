namespace Ubs.Monitoring.Domain.Entities;

public class FxRate
{
    public Guid Id { get; private set; }
    public string BaseCurrencyCode { get; private set; } = null!;
    public string QuoteCurrencyCode { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTimeOffset AsOfUtc { get; private set; }
}
