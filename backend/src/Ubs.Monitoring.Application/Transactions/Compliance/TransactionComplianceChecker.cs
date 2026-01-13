using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Ubs.Monitoring.Application.Cases;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Application.Transactions.Repositories;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Transactions.Compliance;

public sealed class TransactionComplianceChecker : ITransactionComplianceChecker
{
    private readonly IComplianceRuleRepository _rules;
    private readonly ITransactionRepository _transactions;
    private readonly ICaseRepository _cases;
    private readonly ILogger<TransactionComplianceChecker> _logger;

    public TransactionComplianceChecker(
        IComplianceRuleRepository rules,
        ITransactionRepository transactions,
        ICaseRepository cases,
        ILogger<TransactionComplianceChecker> logger)
    {
        _rules = rules;
        _transactions = transactions;
        _cases = cases;
        _logger = logger;
    }

    public async Task CheckAndCreateCaseIfNeededAsync(Transaction tx, CancellationToken ct)
    {
        try
        {
            // Check if a case already exists for this transaction
            var existingCase = await _cases.GetByTransactionIdAsync(tx.Id, ct);
            if (existingCase is not null)
            {
                _logger.LogDebug("Case already exists for transaction {TransactionId}, skipping compliance check", tx.Id);
                return;
            }

            var rules = await _rules.SearchAsync(
                new ComplianceRuleQuery
                {
                    IsActive = true,
                    Page = new() { Page = 1, PageSize = 100 }
                },
                ct
            );

            var violations = new List<ComplianceViolation>();

            foreach (var rule in rules.Items)
            {
                var violation = await EvaluateRuleAsync(rule, tx, ct);
                if (violation is null)
                    continue;

                violations.Add(violation);
                LogViolation(tx, violation);
            }

            // If violations found, create case with findings
            if (violations.Count > 0)
            {
                await CreateCaseWithFindingsAsync(tx, violations, ct);
            }
        }
        catch (Exception ex)
        {
            // Compliance check must never break transaction flow
            _logger.LogError(
                ex,
                "Compliance evaluation failed for transaction {TransactionId}",
                tx.Id
            );
        }
    }

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

    private async Task CreateCaseWithFindingsAsync(
        Transaction tx,
        List<ComplianceViolation> violations,
        CancellationToken ct)
    {
        try
        {
            // Calculate aggregate severity (maximum severity from all violations)
            var maxSeverity = violations.Max(v => v.Severity);

            // Create the case
            var caseEntity = new Case(
                transactionId: tx.Id,
                clientId: tx.ClientId,
                accountId: tx.AccountId,
                initialSeverity: maxSeverity,
                analystId: null // No analyst assigned initially (status will be New)
            );

            _cases.Add(caseEntity);

            // Create a finding for each violation
            foreach (var violation in violations)
            {
                var evidence = JsonDocument.Parse(JsonSerializer.Serialize(new
                {
                    ruleCode = violation.RuleCode,
                    message = violation.Message,
                    transactionId = tx.Id,
                    transactionType = tx.Type.ToString(),
                    amount = tx.Amount,
                    currencyCode = tx.CurrencyCode,
                    baseAmount = tx.BaseAmount,
                    occurredAtUtc = tx.OccurredAtUtc,
                    counterpartyCountry = tx.CpCountryCode,
                    counterpartyName = tx.CpName,
                    counterpartyIdentifier = tx.CpIdentifier,
                    evaluatedAtUtc = DateTimeOffset.UtcNow
                }));

                var finding = new CaseFinding(
                    caseId: caseEntity.Id,
                    ruleId: violation.RuleId,
                    ruleType: violation.RuleType,
                    severity: violation.Severity,
                    evidenceJson: evidence
                );

                _cases.AddFinding(finding);
            }

            await _cases.SaveChangesAsync(ct);

            _logger.LogWarning(
                "CASE_CREATED | CaseId={CaseId} | Tx={TransactionId} | Client={ClientId} | Severity={Severity} | ViolationCount={ViolationCount}",
                caseEntity.Id,
                tx.Id,
                tx.ClientId,
                maxSeverity,
                violations.Count
            );
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Concurrent request already created a case for this transaction
            _logger.LogInformation(
                "Case already created by concurrent request for transaction {TransactionId}. Skipping duplicate creation.",
                tx.Id
            );
        }
    }

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
