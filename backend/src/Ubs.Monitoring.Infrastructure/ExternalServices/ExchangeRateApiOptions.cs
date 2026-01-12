namespace Ubs.Monitoring.Infrastructure.ExternalServices;

/// <summary>
/// Configuration options for the ExchangeRate-API integration.
/// </summary>
public sealed class ExchangeRateApiOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "ExchangeRateApi";

    /// <summary>
    /// API key for ExchangeRate-API.
    /// Can be set via environment variable EXCHANGERATE_API_KEY.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the ExchangeRate-API.
    /// Default: https://v6.exchangerate-api.com/v6
    /// </summary>
    public string BaseUrl { get; set; } = "https://v6.exchangerate-api.com/v6";

    /// <summary>
    /// Cache duration in minutes.
    /// Exchange rates typically update once per day, so caching is highly beneficial.
    /// Default: 60 minutes (1 hour).
    /// </summary>
    public int CacheMinutes { get; set; } = 60;

    /// <summary>
    /// HTTP request timeout in seconds.
    /// Default: 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for transient failures.
    /// Default: 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for exponential backoff between retries.
    /// Actual delay = BaseRetryDelayMs * 2^attemptNumber.
    /// Default: 500ms.
    /// </summary>
    public int BaseRetryDelayMs { get; set; } = 500;

    /// <summary>
    /// Whether to use the database as a fallback when the external API is unavailable.
    /// Default: true.
    /// </summary>
    public bool UseDatabaseFallback { get; set; } = true;
}
