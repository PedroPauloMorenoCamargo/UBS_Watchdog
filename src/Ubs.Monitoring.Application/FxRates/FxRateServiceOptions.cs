namespace Ubs.Monitoring.Application.FxRates;

/// <summary>
/// Configuration options for FX Rate service.
/// </summary>
public sealed class FxRateServiceOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "FxRateService";

    /// <summary>
    /// Time window in minutes to consider an existing FX rate as "recent enough" to reuse.
    /// This prevents creating duplicate rates for the same pair within a short period.
    /// Default: 60 minutes.
    /// </summary>
    public int RateReuseWindowMinutes { get; set; } = 60;

    /// <summary>
    /// The base currency code for all conversions.
    /// Default: USD.
    /// </summary>
    public string BaseCurrencyCode { get; set; } = "USD";
}
