using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common.FileExport;
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

        var rows = new List<List<string>>();

        // Header section
        rows.Add(new List<string> { "CLIENT REPORT", "" });
        rows.Add(new List<string> { "Client", report.ClientName });
        rows.Add(new List<string> { "Client ID", report.ClientId.ToString() });
        rows.Add(new List<string> { "Country", report.CountryCode });
        rows.Add(new List<string> { "Risk Level", report.RiskLevel.ToString() });
        rows.Add(new List<string> { "Period", $"{report.PeriodStart} to {report.PeriodEnd}" });
        rows.Add(new List<string> { "", "" });

        // Transaction Metrics section
        rows.Add(new List<string> { "TRANSACTION METRICS", "" });
        rows.Add(new List<string> { "Total Transactions", report.TransactionMetrics.TotalTransactions.ToString() });
        rows.Add(new List<string> { "Total Volume (USD)", report.TransactionMetrics.TotalVolumeUSD.ToString("N2") });
        rows.Add(new List<string> { "Average Transaction (USD)", report.TransactionMetrics.AverageTransactionUSD.ToString("N2") });
        rows.Add(new List<string> { "Deposits", report.TransactionMetrics.DepositCount.ToString() });
        rows.Add(new List<string> { "Withdrawals", report.TransactionMetrics.WithdrawalCount.ToString() });
        rows.Add(new List<string> { "Transfers", report.TransactionMetrics.TransferCount.ToString() });
        rows.Add(new List<string> { "", "" });

        // Case Metrics section
        rows.Add(new List<string> { "CASE METRICS", "" });
        rows.Add(new List<string> { "Total Cases", report.CaseMetrics.TotalCases.ToString() });
        rows.Add(new List<string> { "New", report.CaseMetrics.NewCases.ToString() });
        rows.Add(new List<string> { "Under Review", report.CaseMetrics.UnderReviewCases.ToString() });
        rows.Add(new List<string> { "Resolved", report.CaseMetrics.ResolvedCases.ToString() });
        rows.Add(new List<string> { "Fraudulent", report.CaseMetrics.FraudulentCases.ToString() });
        rows.Add(new List<string> { "Not Fraudulent", report.CaseMetrics.NotFraudulentCases.ToString() });
        rows.Add(new List<string> { "Inconclusive", report.CaseMetrics.InconclusiveCases.ToString() });
        rows.Add(new List<string> { "", "" });

        // Cases by Severity section
        rows.Add(new List<string> { "CASES BY SEVERITY", "" });
        rows.Add(new List<string> { "Severity", "Count" });
        foreach (var item in report.CasesBySeverity)
        {
            rows.Add(new List<string> { item.Severity.ToString(), item.Count.ToString() });
        }
        rows.Add(new List<string> { "", "" });

        // Transactions by Type section
        rows.Add(new List<string> { "TRANSACTIONS BY TYPE", "", "" });
        rows.Add(new List<string> { "Type", "Count", "Volume (USD)" });
        foreach (var item in report.TransactionsByType)
        {
            rows.Add(new List<string> { item.Type.ToString(), item.Count.ToString(), item.VolumeUSD.ToString("N2") });
        }
        rows.Add(new List<string> { "", "", "" });

        // Transaction Trend section
        rows.Add(new List<string> { "TRANSACTION TREND", "", "" });
        rows.Add(new List<string> { "Date", "Count", "Volume (USD)" });
        foreach (var item in report.TransactionTrend)
        {
            rows.Add(new List<string> { item.Date.ToString(), item.Count.ToString(), item.VolumeUSD.ToString("N2") });
        }
        rows.Add(new List<string> { "", "", "" });

        // Top Accounts section
        rows.Add(new List<string> { "TOP ACCOUNTS BY VOLUME", "", "" });
        rows.Add(new List<string> { "Account Identifier", "Transactions", "Total Volume (USD)" });
        foreach (var item in report.TopAccounts)
        {
            rows.Add(new List<string> { item.AccountIdentifier, item.TransactionCount.ToString(), item.TotalVolumeUSD.ToString("N2") });
        }

        // Export using CsvExportHelper
        // Note: Report has variable columns, normalized to 3 columns max
        var csvBytes = CsvExportHelper.ExportToCsvRaw(
            headers: new List<string>(), // No consistent header for multi-section report
            rows: rows
        );

        return Encoding.UTF8.GetString(csvBytes);
    }

    public async Task<string> GenerateSystemReportCsvAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken ct)
    {
        var report = await GetSystemReportAsync(startDate, endDate, ct);

        var rows = new List<List<string>>();

        // Header section
        rows.Add(new List<string> { "SYSTEM REPORT", "", "", "" });
        rows.Add(new List<string> { "Period", $"{report.PeriodStart} to {report.PeriodEnd}", "", "" });
        rows.Add(new List<string> { "Total Clients", report.TotalClients.ToString(), "", "" });
        rows.Add(new List<string> { "Active Clients", report.ActiveClients.ToString(), "", "" });
        rows.Add(new List<string> { "", "", "", "" });

        // Transaction Metrics section
        rows.Add(new List<string> { "TRANSACTION METRICS", "", "", "" });
        rows.Add(new List<string> { "Total Transactions", report.TransactionMetrics.TotalTransactions.ToString(), "", "" });
        rows.Add(new List<string> { "Total Volume (USD)", report.TransactionMetrics.TotalVolumeUSD.ToString("N2"), "", "" });
        rows.Add(new List<string> { "Average Transaction (USD)", report.TransactionMetrics.AverageTransactionUSD.ToString("N2"), "", "" });
        rows.Add(new List<string> { "Deposits", report.TransactionMetrics.DepositCount.ToString(), "", "" });
        rows.Add(new List<string> { "Withdrawals", report.TransactionMetrics.WithdrawalCount.ToString(), "", "" });
        rows.Add(new List<string> { "Transfers", report.TransactionMetrics.TransferCount.ToString(), "", "" });
        rows.Add(new List<string> { "", "", "", "" });

        // Case Metrics section
        rows.Add(new List<string> { "CASE METRICS", "", "", "" });
        rows.Add(new List<string> { "Total Cases", report.CaseMetrics.TotalCases.ToString(), "", "" });
        rows.Add(new List<string> { "New", report.CaseMetrics.NewCases.ToString(), "", "" });
        rows.Add(new List<string> { "Under Review", report.CaseMetrics.UnderReviewCases.ToString(), "", "" });
        rows.Add(new List<string> { "Resolved", report.CaseMetrics.ResolvedCases.ToString(), "", "" });
        rows.Add(new List<string> { "Fraudulent", report.CaseMetrics.FraudulentCases.ToString(), "", "" });
        rows.Add(new List<string> { "Not Fraudulent", report.CaseMetrics.NotFraudulentCases.ToString(), "", "" });
        rows.Add(new List<string> { "Inconclusive", report.CaseMetrics.InconclusiveCases.ToString(), "", "" });
        rows.Add(new List<string> { "", "", "", "" });

        // Cases by Severity section
        rows.Add(new List<string> { "CASES BY SEVERITY", "", "", "" });
        rows.Add(new List<string> { "Severity", "Count", "", "" });
        foreach (var item in report.CasesBySeverity)
        {
            rows.Add(new List<string> { item.Severity.ToString(), item.Count.ToString(), "", "" });
        }
        rows.Add(new List<string> { "", "", "", "" });

        // Transactions by Type section
        rows.Add(new List<string> { "TRANSACTIONS BY TYPE", "", "", "" });
        rows.Add(new List<string> { "Type", "Count", "Volume (USD)", "" });
        foreach (var item in report.TransactionsByType)
        {
            rows.Add(new List<string> { item.Type.ToString(), item.Count.ToString(), item.VolumeUSD.ToString("N2"), "" });
        }
        rows.Add(new List<string> { "", "", "", "" });

        // Transaction Trend section
        rows.Add(new List<string> { "TRANSACTION TREND", "", "", "" });
        rows.Add(new List<string> { "Date", "Count", "Volume (USD)", "" });
        foreach (var item in report.TransactionTrend)
        {
            rows.Add(new List<string> { item.Date.ToString(), item.Count.ToString(), item.VolumeUSD.ToString("N2"), "" });
        }
        rows.Add(new List<string> { "", "", "", "" });

        // Top Clients by Volume section
        rows.Add(new List<string> { "TOP CLIENTS BY VOLUME", "", "", "" });
        rows.Add(new List<string> { "Client Name", "Transactions", "Volume (USD)", "Cases" });
        foreach (var item in report.TopClientsByVolume)
        {
            rows.Add(new List<string> { item.ClientName, item.TransactionCount.ToString(), item.TotalVolumeUSD.ToString("N2"), item.CaseCount.ToString() });
        }
        rows.Add(new List<string> { "", "", "", "" });

        // Top Clients by Cases section
        rows.Add(new List<string> { "TOP CLIENTS BY CASE COUNT", "", "", "" });
        rows.Add(new List<string> { "Client Name", "Cases", "Transactions", "Volume (USD)" });
        foreach (var item in report.TopClientsByCases)
        {
            rows.Add(new List<string> { item.ClientName, item.CaseCount.ToString(), item.TransactionCount.ToString(), item.TotalVolumeUSD.ToString("N2") });
        }

        // Export using CsvExportHelper
        // Note: Report has variable columns, normalized to 4 columns max
        var csvBytes = CsvExportHelper.ExportToCsvRaw(
            headers: new List<string>(), // No consistent header for multi-section report
            rows: rows
        );

        return Encoding.UTF8.GetString(csvBytes);
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
