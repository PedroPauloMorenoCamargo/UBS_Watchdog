namespace Ubs.Monitoring.Domain.Entities;

public class FxRate
{
    private FxRate() { }

    public FxRate(
        string baseCurrencyCode,
        string quoteCurrencyCode,
        decimal rate,
        DateTimeOffset asOfUtc)
    {
        if (string.IsNullOrWhiteSpace(baseCurrencyCode))
            throw new ArgumentException("Base currency code is required", nameof(baseCurrencyCode));
        if (string.IsNullOrWhiteSpace(quoteCurrencyCode))
            throw new ArgumentException("Quote currency code is required", nameof(quoteCurrencyCode));
        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be positive", nameof(rate));

        Id = Guid.NewGuid();
        BaseCurrencyCode = baseCurrencyCode.ToUpperInvariant();
        QuoteCurrencyCode = quoteCurrencyCode.ToUpperInvariant();
        Rate = rate;
        AsOfUtc = asOfUtc;
    }

    public Guid Id { get; private set; }
    public string BaseCurrencyCode { get; private set; } = null!;
    public string QuoteCurrencyCode { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTimeOffset AsOfUtc { get; private set; }
}
