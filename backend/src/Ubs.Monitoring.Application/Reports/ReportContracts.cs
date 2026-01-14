using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Reports;

/// <summary>
/// Client-specific report data.
/// </summary>
public sealed record ClientReportDto(
    Guid ClientId,
    string ClientName,
    string CountryCode,
    RiskLevel RiskLevel,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    TransactionMetricsDto TransactionMetrics,
    CaseMetricsDto CaseMetrics,
    IReadOnlyList<TransactionTrendDto> TransactionTrend,
    IReadOnlyList<CaseBySeverityDto> CasesBySeverity,
    IReadOnlyList<TransactionByTypeDto> TransactionsByType,
    IReadOnlyList<TopAccountDto> TopAccounts
);

/// <summary>
/// System-wide report data (all clients).
/// </summary>
public sealed record SystemReportDto(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int TotalClients,
    int ActiveClients,
    TransactionMetricsDto TransactionMetrics,
    CaseMetricsDto CaseMetrics,
    IReadOnlyList<TransactionTrendDto> TransactionTrend,
    IReadOnlyList<CaseBySeverityDto> CasesBySeverity,
    IReadOnlyList<TransactionByTypeDto> TransactionsByType,
    IReadOnlyList<ClientRankingDto> TopClientsByVolume,
    IReadOnlyList<ClientRankingDto> TopClientsByCases
);

/// <summary>
/// Transaction metrics summary.
/// </summary>
public sealed record TransactionMetricsDto(
    int TotalTransactions,
    decimal TotalVolumeUSD,
    decimal AverageTransactionUSD,
    int DepositCount,
    int WithdrawalCount,
    int TransferCount
);

/// <summary>
/// Case metrics summary.
/// </summary>
public sealed record CaseMetricsDto(
    int TotalCases,
    int NewCases,
    int UnderReviewCases,
    int ResolvedCases,
    int FraudulentCases,
    int NotFraudulentCases,
    int InconclusiveCases,
    int LowSeverityCases,
    int MediumSeverityCases,
    int HighSeverityCases,
    int CriticalSeverityCases
);

/// <summary>
/// Transaction trend data point (for line chart).
/// </summary>
public sealed record TransactionTrendDto(
    DateOnly Date,
    int Count,
    decimal VolumeUSD
);

/// <summary>
/// Cases by severity data point (for pie/bar chart).
/// </summary>
public sealed record CaseBySeverityDto(
    Severity Severity,
    int Count
);

/// <summary>
/// Transactions by type data point (for pie/bar chart).
/// </summary>
public sealed record TransactionByTypeDto(
    TransactionType Type,
    int Count,
    decimal VolumeUSD
);

/// <summary>
/// Top account by volume.
/// </summary>
public sealed record TopAccountDto(
    Guid AccountId,
    string AccountIdentifier,
    int TransactionCount,
    decimal TotalVolumeUSD
);

/// <summary>
/// Client ranking (for system report).
/// </summary>
public sealed record ClientRankingDto(
    Guid ClientId,
    string ClientName,
    int TransactionCount,
    decimal TotalVolumeUSD,
    int CaseCount
);
