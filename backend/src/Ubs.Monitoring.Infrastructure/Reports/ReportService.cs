using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Reports;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Reports;

/// <summary>
/// Service implementation for generating reports and analytics.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly IClientRepository _clients;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        AppDbContext db,
        IClientRepository clients,
        ILogger<ReportService> logger)
    {
        _db = db;
        _clients = clients;
        _logger = logger;
    }

    public async Task<ClientReportDto?> GetClientReportAsync(
        Guid clientId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct)
    {
        var client = await _clients.GetByIdAsync(clientId, ct);
        if (client is null)
        {
            _logger.LogWarning("Client report requested for non-existent client {ClientId}", clientId);
            return null;
        }

        var (start, end) = NormalizeDateRange(startDate, endDate);

        _logger.LogInformation("Generating client report for {ClientId} from {Start} to {End}",
            clientId, start, end);

        // Get transactions for the period
        var transactions = await _db.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Where(t => t.ClientId == clientId
                && DateOnly.FromDateTime(t.OccurredAtUtc.UtcDateTime) >= start
                && DateOnly.FromDateTime(t.OccurredAtUtc.UtcDateTime) <= end)
            .ToListAsync(ct);

        // Get cases for the period
        var cases = await _db.Cases
            .AsNoTracking()
            .Where(c => c.ClientId == clientId
                && DateOnly.FromDateTime(c.OpenedAtUtc.UtcDateTime) >= start
                && DateOnly.FromDateTime(c.OpenedAtUtc.UtcDateTime) <= end)
            .ToListAsync(ct);

        // Calculate metrics
        var transactionMetrics = new TransactionMetricsDto(
            TotalTransactions: transactions.Count,
            TotalVolumeUSD: transactions.Sum(t => t.BaseAmount),
            AverageTransactionUSD: transactions.Count > 0 ? transactions.Average(t => t.BaseAmount) : 0,
            DepositCount: transactions.Count(t => t.Type == TransactionType.Deposit),
            WithdrawalCount: transactions.Count(t => t.Type == TransactionType.Withdrawal),
            TransferCount: transactions.Count(t => t.Type == TransactionType.Transfer)
        );

        var caseMetrics = new CaseMetricsDto(
            TotalCases: cases.Count,
            NewCases: cases.Count(c => c.Status == CaseStatus.New),
            UnderReviewCases: cases.Count(c => c.Status == CaseStatus.UnderReview),
            ResolvedCases: cases.Count(c => c.Status == CaseStatus.Resolved),
            FraudulentCases: cases.Count(c => c.Decision == CaseDecision.Fraudulent),
            NotFraudulentCases: cases.Count(c => c.Decision == CaseDecision.NotFraudulent),
            InconclusiveCases: cases.Count(c => c.Decision == CaseDecision.Inconclusive),
            LowSeverityCases: cases.Count(c => c.Severity == Severity.Low),
            MediumSeverityCases: cases.Count(c => c.Severity == Severity.Medium),
            HighSeverityCases: cases.Count(c => c.Severity == Severity.High),
            CriticalSeverityCases: cases.Count(c => c.Severity == Severity.Critical)
        );

        // Transaction trend (line chart data)
        var transactionTrend = transactions
            .GroupBy(t => DateOnly.FromDateTime(t.OccurredAtUtc.UtcDateTime))
            .Select(g => new TransactionTrendDto(
                Date: g.Key,
                Count: g.Count(),
                VolumeUSD: g.Sum(t => t.BaseAmount)
            ))
            .OrderBy(t => t.Date)
            .ToList();

        // Cases by severity (pie/bar chart data)
        var casesBySeverity = cases
            .GroupBy(c => c.Severity)
            .Select(g => new CaseBySeverityDto(
                Severity: g.Key,
                Count: g.Count()
            ))
            .OrderBy(c => c.Severity)
            .ToList();

        // Transactions by type (pie/bar chart data)
        var transactionsByType = transactions
            .GroupBy(t => t.Type)
            .Select(g => new TransactionByTypeDto(
                Type: g.Key,
                Count: g.Count(),
                VolumeUSD: g.Sum(t => t.BaseAmount)
            ))
            .OrderBy(t => t.Type)
            .ToList();

        // Top accounts by volume (grouping in-memory)
        var topAccounts = transactions
            .GroupBy(t => new { t.AccountId, t.Account.AccountIdentifier })
            .Select(g => new TopAccountDto(
                g.Key.AccountId,
                g.Key.AccountIdentifier,
                g.Count(),
                g.Sum(t => t.BaseAmount)
            ))
            .OrderByDescending(a => a.TotalVolumeUSD)
            .Take(10)
            .ToList();

        return new ClientReportDto(
            ClientId: client.Id,
            ClientName: client.Name,
            CountryCode: client.CountryCode,
            RiskLevel: client.RiskLevel,
            PeriodStart: start,
            PeriodEnd: end,
            TransactionMetrics: transactionMetrics,
            CaseMetrics: caseMetrics,
            TransactionTrend: transactionTrend,
            CasesBySeverity: casesBySeverity,
            TransactionsByType: transactionsByType,
            TopAccounts: topAccounts
        );
    }

    public async Task<SystemReportDto> GetSystemReportAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct)
    {
        var (start, end) = NormalizeDateRange(startDate, endDate);

        _logger.LogInformation("Generating system report from {Start} to {End}", start, end);

        // Get all transactions for the period
        var transactions = await _db.Transactions
            .AsNoTracking()
            .Include(t => t.Client)
            .Where(t => DateOnly.FromDateTime(t.OccurredAtUtc.UtcDateTime) >= start
                && DateOnly.FromDateTime(t.OccurredAtUtc.UtcDateTime) <= end)
            .ToListAsync(ct);

        // Get all cases for the period
        var cases = await _db.Cases
            .AsNoTracking()
            .Include(c => c.Client)
            .Where(c => DateOnly.FromDateTime(c.OpenedAtUtc.UtcDateTime) >= start
                && DateOnly.FromDateTime(c.OpenedAtUtc.UtcDateTime) <= end)
            .ToListAsync(ct);

        // Count total and active clients
        var totalClients = await _db.Clients.CountAsync(ct);
        var activeClientIds = transactions.Select(t => t.ClientId).Distinct().ToList();
        var activeClients = activeClientIds.Count;

        // Calculate metrics
        var transactionMetrics = new TransactionMetricsDto(
            TotalTransactions: transactions.Count,
            TotalVolumeUSD: transactions.Sum(t => t.BaseAmount),
            AverageTransactionUSD: transactions.Count > 0 ? transactions.Average(t => t.BaseAmount) : 0,
            DepositCount: transactions.Count(t => t.Type == TransactionType.Deposit),
            WithdrawalCount: transactions.Count(t => t.Type == TransactionType.Withdrawal),
            TransferCount: transactions.Count(t => t.Type == TransactionType.Transfer)
        );

        var caseMetrics = new CaseMetricsDto(
            TotalCases: cases.Count,
            NewCases: cases.Count(c => c.Status == CaseStatus.New),
            UnderReviewCases: cases.Count(c => c.Status == CaseStatus.UnderReview),
            ResolvedCases: cases.Count(c => c.Status == CaseStatus.Resolved),
            FraudulentCases: cases.Count(c => c.Decision == CaseDecision.Fraudulent),
            NotFraudulentCases: cases.Count(c => c.Decision == CaseDecision.NotFraudulent),
            InconclusiveCases: cases.Count(c => c.Decision == CaseDecision.Inconclusive),
            LowSeverityCases: cases.Count(c => c.Severity == Severity.Low),
            MediumSeverityCases: cases.Count(c => c.Severity == Severity.Medium),
            HighSeverityCases: cases.Count(c => c.Severity == Severity.High),
            CriticalSeverityCases: cases.Count(c => c.Severity == Severity.Critical)
        );

        // Transaction trend (line chart data)
        var transactionTrend = transactions
            .GroupBy(t => DateOnly.FromDateTime(t.OccurredAtUtc.UtcDateTime))
            .Select(g => new TransactionTrendDto(
                Date: g.Key,
                Count: g.Count(),
                VolumeUSD: g.Sum(t => t.BaseAmount)
            ))
            .OrderBy(t => t.Date)
            .ToList();

        // Cases by severity (pie/bar chart data)
        var casesBySeverity = cases
            .GroupBy(c => c.Severity)
            .Select(g => new CaseBySeverityDto(
                Severity: g.Key,
                Count: g.Count()
            ))
            .OrderBy(c => c.Severity)
            .ToList();

        // Transactions by type (pie/bar chart data)
        var transactionsByType = transactions
            .GroupBy(t => t.Type)
            .Select(g => new TransactionByTypeDto(
                Type: g.Key,
                Count: g.Count(),
                VolumeUSD: g.Sum(t => t.BaseAmount)
            ))
            .OrderBy(t => t.Type)
            .ToList();

        // Top clients by transaction volume (grouping in-memory)
        var topClientsByVolume = transactions
            .GroupBy(t => new { t.ClientId, t.Client.Name })
            .Select(g => new ClientRankingDto(
                g.Key.ClientId,
                g.Key.Name,
                g.Count(),
                g.Sum(t => t.BaseAmount),
                0 // Will be filled separately
            ))
            .OrderByDescending(c => c.TotalVolumeUSD)
            .Take(10)
            .ToList();

        // Top clients by case count (grouping in-memory)
        var topClientsByCases = cases
            .GroupBy(c => new { c.ClientId, c.Client.Name })
            .Select(g => new
            {
                g.Key.ClientId,
                g.Key.Name,
                CaseCount = g.Count()
            })
            .OrderByDescending(c => c.CaseCount)
            .Take(10)
            .ToList();

        // Fill case counts for top clients by volume
        var topClientsByVolumeWithCases = topClientsByVolume.Select(c =>
        {
            var caseCount = cases.Count(cs => cs.ClientId == c.ClientId);
            return c with { CaseCount = caseCount };
        }).ToList();

        // Build top clients by cases with transaction data
        var topClientsByCasesWithData = new List<ClientRankingDto>();
        foreach (var client in topClientsByCases)
        {
            var txData = transactions.Where(t => t.ClientId == client.ClientId).ToList();
            topClientsByCasesWithData.Add(new ClientRankingDto(
                ClientId: client.ClientId,
                ClientName: client.Name,
                TransactionCount: txData.Count,
                TotalVolumeUSD: txData.Sum(t => t.BaseAmount),
                CaseCount: client.CaseCount
            ));
        }

        return new SystemReportDto(
            PeriodStart: start,
            PeriodEnd: end,
            TotalClients: totalClients,
            ActiveClients: activeClients,
            TransactionMetrics: transactionMetrics,
            CaseMetrics: caseMetrics,
            TransactionTrend: transactionTrend,
            CasesBySeverity: casesBySeverity,
            TransactionsByType: transactionsByType,
            TopClientsByVolume: topClientsByVolumeWithCases,
            TopClientsByCases: topClientsByCasesWithData
        );
    }

    public async Task<string?> GenerateClientReportCsvAsync(
        Guid clientId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct)
    {
        var report = await GetClientReportAsync(clientId, startDate, endDate, ct);
        if (report is null)
            return null;

        var csv = new StringBuilder();

        // Header
        csv.AppendLine("CLIENT REPORT");
        csv.AppendLine($"Client,{report.ClientName}");
        csv.AppendLine($"Client ID,{report.ClientId}");
        csv.AppendLine($"Country,{report.CountryCode}");
        csv.AppendLine($"Risk Level,{report.RiskLevel}");
        csv.AppendLine($"Period,{report.PeriodStart} to {report.PeriodEnd}");
        csv.AppendLine();

        // Transaction Metrics
        csv.AppendLine("TRANSACTION METRICS");
        csv.AppendLine($"Total Transactions,{report.TransactionMetrics.TotalTransactions}");
        csv.AppendLine($"Total Volume (USD),{report.TransactionMetrics.TotalVolumeUSD:N2}");
        csv.AppendLine($"Average Transaction (USD),{report.TransactionMetrics.AverageTransactionUSD:N2}");
        csv.AppendLine($"Deposits,{report.TransactionMetrics.DepositCount}");
        csv.AppendLine($"Withdrawals,{report.TransactionMetrics.WithdrawalCount}");
        csv.AppendLine($"Transfers,{report.TransactionMetrics.TransferCount}");
        csv.AppendLine();

        // Case Metrics
        csv.AppendLine("CASE METRICS");
        csv.AppendLine($"Total Cases,{report.CaseMetrics.TotalCases}");
        csv.AppendLine($"New,{report.CaseMetrics.NewCases}");
        csv.AppendLine($"Under Review,{report.CaseMetrics.UnderReviewCases}");
        csv.AppendLine($"Resolved,{report.CaseMetrics.ResolvedCases}");
        csv.AppendLine($"Fraudulent,{report.CaseMetrics.FraudulentCases}");
        csv.AppendLine($"Not Fraudulent,{report.CaseMetrics.NotFraudulentCases}");
        csv.AppendLine($"Inconclusive,{report.CaseMetrics.InconclusiveCases}");
        csv.AppendLine();

        // Cases by Severity
        csv.AppendLine("CASES BY SEVERITY");
        csv.AppendLine("Severity,Count");
        foreach (var item in report.CasesBySeverity)
        {
            csv.AppendLine($"{item.Severity},{item.Count}");
        }
        csv.AppendLine();

        // Transactions by Type
        csv.AppendLine("TRANSACTIONS BY TYPE");
        csv.AppendLine("Type,Count,Volume (USD)");
        foreach (var item in report.TransactionsByType)
        {
            csv.AppendLine($"{item.Type},{item.Count},{item.VolumeUSD:N2}");
        }
        csv.AppendLine();

        // Transaction Trend
        csv.AppendLine("TRANSACTION TREND");
        csv.AppendLine("Date,Count,Volume (USD)");
        foreach (var item in report.TransactionTrend)
        {
            csv.AppendLine($"{item.Date},{item.Count},{item.VolumeUSD:N2}");
        }
        csv.AppendLine();

        // Top Accounts
        csv.AppendLine("TOP ACCOUNTS BY VOLUME");
        csv.AppendLine("Account Identifier,Transactions,Total Volume (USD)");
        foreach (var item in report.TopAccounts)
        {
            csv.AppendLine($"{item.AccountIdentifier},{item.TransactionCount},{item.TotalVolumeUSD:N2}");
        }

        return csv.ToString();
    }

    public async Task<string> GenerateSystemReportCsvAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct)
    {
        var report = await GetSystemReportAsync(startDate, endDate, ct);

        var csv = new StringBuilder();

        // Header
        csv.AppendLine("SYSTEM REPORT");
        csv.AppendLine($"Period,{report.PeriodStart} to {report.PeriodEnd}");
        csv.AppendLine($"Total Clients,{report.TotalClients}");
        csv.AppendLine($"Active Clients,{report.ActiveClients}");
        csv.AppendLine();

        // Transaction Metrics
        csv.AppendLine("TRANSACTION METRICS");
        csv.AppendLine($"Total Transactions,{report.TransactionMetrics.TotalTransactions}");
        csv.AppendLine($"Total Volume (USD),{report.TransactionMetrics.TotalVolumeUSD:N2}");
        csv.AppendLine($"Average Transaction (USD),{report.TransactionMetrics.AverageTransactionUSD:N2}");
        csv.AppendLine($"Deposits,{report.TransactionMetrics.DepositCount}");
        csv.AppendLine($"Withdrawals,{report.TransactionMetrics.WithdrawalCount}");
        csv.AppendLine($"Transfers,{report.TransactionMetrics.TransferCount}");
        csv.AppendLine();

        // Case Metrics
        csv.AppendLine("CASE METRICS");
        csv.AppendLine($"Total Cases,{report.CaseMetrics.TotalCases}");
        csv.AppendLine($"New,{report.CaseMetrics.NewCases}");
        csv.AppendLine($"Under Review,{report.CaseMetrics.UnderReviewCases}");
        csv.AppendLine($"Resolved,{report.CaseMetrics.ResolvedCases}");
        csv.AppendLine($"Fraudulent,{report.CaseMetrics.FraudulentCases}");
        csv.AppendLine($"Not Fraudulent,{report.CaseMetrics.NotFraudulentCases}");
        csv.AppendLine($"Inconclusive,{report.CaseMetrics.InconclusiveCases}");
        csv.AppendLine();

        // Cases by Severity
        csv.AppendLine("CASES BY SEVERITY");
        csv.AppendLine("Severity,Count");
        foreach (var item in report.CasesBySeverity)
        {
            csv.AppendLine($"{item.Severity},{item.Count}");
        }
        csv.AppendLine();

        // Transactions by Type
        csv.AppendLine("TRANSACTIONS BY TYPE");
        csv.AppendLine("Type,Count,Volume (USD)");
        foreach (var item in report.TransactionsByType)
        {
            csv.AppendLine($"{item.Type},{item.Count},{item.VolumeUSD:N2}");
        }
        csv.AppendLine();

        // Transaction Trend
        csv.AppendLine("TRANSACTION TREND");
        csv.AppendLine("Date,Count,Volume (USD)");
        foreach (var item in report.TransactionTrend)
        {
            csv.AppendLine($"{item.Date},{item.Count},{item.VolumeUSD:N2}");
        }
        csv.AppendLine();

        // Top Clients by Volume
        csv.AppendLine("TOP CLIENTS BY VOLUME");
        csv.AppendLine("Client Name,Transactions,Volume (USD),Cases");
        foreach (var item in report.TopClientsByVolume)
        {
            csv.AppendLine($"{item.ClientName},{item.TransactionCount},{item.TotalVolumeUSD:N2},{item.CaseCount}");
        }
        csv.AppendLine();

        // Top Clients by Cases
        csv.AppendLine("TOP CLIENTS BY CASE COUNT");
        csv.AppendLine("Client Name,Cases,Transactions,Volume (USD)");
        foreach (var item in report.TopClientsByCases)
        {
            csv.AppendLine($"{item.ClientName},{item.CaseCount},{item.TransactionCount},{item.TotalVolumeUSD:N2}");
        }

        return csv.ToString();
    }

    #region Private Helpers

    private static (DateOnly Start, DateOnly End) NormalizeDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        if (start > end)
        {
            (start, end) = (end, start);
        }

        return (start, end);
    }

    #endregion
}
