using System.Text.Json;
using Microsoft.Extensions.Logging;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Application.Transactions.Repositories;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Transactions.Compliance;

public sealed class TransactionComplianceChecker : ITransactionComplianceChecker
{
    private readonly IComplianceRuleRepository _rules;
    private readonly ITransactionRepository _transactions;
    private readonly ILogger<TransactionComplianceChecker> _logger;

    public TransactionComplianceChecker(
        IComplianceRuleRepository rules,
        ITransactionRepository transactions,
        ILogger<TransactionComplianceChecker> logger)
    {
        _rules = rules;
        _transactions = transactions;
        _logger = logger;
    }

    public async Task CheckAsync(Transaction tx, CancellationToken ct)
    {
        try
        {
            var rules = await _rules.SearchAsync(
                new ComplianceRuleQuery
                {
                    IsActive = true,
                    Page = new() { Page = 1, PageSize = 100 }
                },
                ct
            );

            foreach (var rule in rules.Items)
            {
                var violation = await EvaluateRuleAsync(rule, tx, ct);
                if (violation is null)
                    continue;

                LogViolation(tx, violation);
            }
        }
        catch (Exception ex)
        {
            // Compliance must NEVER break the transaction flow
            _logger.LogError(
                ex,
                "Compliance evaluation failed for transaction {TransactionId}",
                tx.Id
            );
        }
    }

    // -----------------------
    // Rule evaluation
    // -----------------------

    private async Task<ComplianceViolation?> EvaluateRuleAsync(
        ComplianceRule rule,
        Transaction tx,
        CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(rule.ParametersJson);

        return rule.RuleType switch
        {
            RuleType.DailyLimit =>
                await CheckDailyLimit(rule, doc.RootElement, tx, ct),

            RuleType.BannedCountries =>
                CheckBannedCountries(rule, doc.RootElement, tx),

            RuleType.Structuring =>
                await CheckStructuring(rule, doc.RootElement, tx, ct),

            _ => null
        };
    }

    // -----------------------
    // Individual rules
    // -----------------------

    private async Task<ComplianceViolation?> CheckDailyLimit(
        ComplianceRule rule,
        JsonElement parameters,
        Transaction tx,
        CancellationToken ct)
    {
        var limit = parameters.GetProperty("limitBaseAmount").GetDecimal();
        var date = DateOnly.FromDateTime(tx.OccurredAtUtc.UtcDateTime);

        var total = rule.Scope == "PerAccount"
            ? await _transactions.GetDailyTotalByAccountAsync(tx.AccountId, date, ct)
            : await _transactions.GetDailyTotalByClientAsync(tx.ClientId, date, ct);

        if (total <= limit)
            return null;

        return new ComplianceViolation(
            rule.Id,
            rule.Code,
            rule.RuleType,
            rule.Severity,
            $"Daily limit exceeded: {total} > {limit}"
        );
    }

    private ComplianceViolation? CheckBannedCountries(
        ComplianceRule rule,
        JsonElement parameters,
        Transaction tx)
    {
        if (string.IsNullOrWhiteSpace(tx.CpCountryCode))
            return null;

        foreach (var c in parameters.GetProperty("countries").EnumerateArray())
        {
            if (string.Equals(c.GetString(), tx.CpCountryCode, StringComparison.OrdinalIgnoreCase))
            {
                return new ComplianceViolation(
                    rule.Id,
                    rule.Code,
                    rule.RuleType,
                    rule.Severity,
                    $"Transaction involves banned country {tx.CpCountryCode}"
                );
            }
        }

        return null;
    }

    private async Task<ComplianceViolation?> CheckStructuring(
        ComplianceRule rule,
        JsonElement parameters,
        Transaction tx,
        CancellationToken ct)
    {
        var n = parameters.GetProperty("n").GetInt32();
        var max = parameters.GetProperty("xBaseAmount").GetDecimal();
        var date = DateOnly.FromDateTime(tx.OccurredAtUtc.UtcDateTime);

        var count = rule.Scope == "PerAccount"
            ? await _transactions.CountDailyTransfersUnderBaseAmountByAccountAsync(tx.AccountId, date, max, ct)
            : await _transactions.CountDailyTransfersUnderBaseAmountByClientAsync(tx.ClientId, date, max, ct);

        if (count < n)
            return null;

        return new ComplianceViolation(
            rule.Id,
            rule.Code,
            rule.RuleType,
            rule.Severity,
            $"Structuring detected: {count} transfers under {max} in one day"
        );
    }

    // -----------------------
    // Logging
    // -----------------------

    private void LogViolation(Transaction tx, ComplianceViolation v)
    {
        _logger.LogWarning(
            "COMPLIANCE_VIOLATION | Tx={TransactionId} | Rule={RuleCode} | Severity={Severity} | Client={ClientId} | Account={AccountId} | {Message}",
            tx.Id,
            v.RuleCode,
            v.Severity,
            tx.ClientId,
            tx.AccountId,
            v.Message
        );
    }
}
