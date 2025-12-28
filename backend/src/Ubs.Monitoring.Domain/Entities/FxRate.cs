namespace Ubs.Monitoring.Domain.Entities;

public class FxRate
{
    public Guid Id { get; set; }
    public string BaseCurrencyCode { get; set; } = null!;
    public string QuoteCurrencyCode { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateTimeOffset AsOfUtc { get; set; }
}
