namespace Ubs.Monitoring.Application.FxRates;

/// <summary>
/// DTO representing a single exchange rate.
/// </summary>
public sealed record ExchangeRateDto(
    string BaseCurrencyCode,
    string QuoteCurrencyCode,
    decimal Rate,
    DateTimeOffset LastUpdatedUtc,
    DateTimeOffset NextUpdateUtc
);

/// <summary>
/// DTO representing all exchange rates for a base currency.
/// </summary>
public sealed record ExchangeRatesResponseDto(
    string BaseCurrencyCode,
    DateTimeOffset LastUpdatedUtc,
    DateTimeOffset NextUpdateUtc,
    IReadOnlyDictionary<string, decimal> ConversionRates
);

#region ExchangeRate-API External DTOs

/// <summary>
/// DTO mapping the raw response from ExchangeRate-API Standard endpoint.
/// GET https://v6.exchangerate-api.com/v6/{API_KEY}/latest/{BASE}
/// </summary>
public sealed class ExchangeRateApiResponse
{
    /// <summary>
    /// Result status: "success" or "error"
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Documentation URL
    /// </summary>
    public string Documentation { get; set; } = string.Empty;

    /// <summary>
    /// Terms of use URL
    /// </summary>
    public string Terms_of_use { get; set; } = string.Empty;

    /// <summary>
    /// Unix timestamp of last update
    /// </summary>
    public long Time_last_update_unix { get; set; }

    /// <summary>
    /// UTC string of last update
    /// </summary>
    public string Time_last_update_utc { get; set; } = string.Empty;

    /// <summary>
    /// Unix timestamp of next update
    /// </summary>
    public long Time_next_update_unix { get; set; }

    /// <summary>
    /// UTC string of next update
    /// </summary>
    public string Time_next_update_utc { get; set; } = string.Empty;

    /// <summary>
    /// Base currency code
    /// </summary>
    public string Base_code { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary of currency codes to their conversion rates
    /// </summary>
    public Dictionary<string, decimal> Conversion_rates { get; set; } = new();

    /// <summary>
    /// Error type (only present when Result is "error")
    /// Possible values: "unsupported-code", "malformed-request", "invalid-key", "inactive-account", "quota-reached"
    /// </summary>
    public string? Error_type { get; set; }
}

#endregion
