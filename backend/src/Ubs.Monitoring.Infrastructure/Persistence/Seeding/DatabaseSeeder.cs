using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Infrastructure.Persistence.Seeding;

/// <summary>
/// Responsible for initializing and seeding the database with
/// required baseline data and optional demo data.
/// </summary>
public sealed class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly SeedOptions _options;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DatabaseSeeder> _logger;
    public DatabaseSeeder(
        AppDbContext db,
        IOptions<SeedOptions> options,
        IPasswordHasher hasher,
        ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _options = options.Value;
        _hasher = hasher;
        _logger = logger;
    }

    /// <summary>
    /// Executes the database seeding process.
    /// </summary>
    /// <remarks>
    /// The execution flow is:
    /// <list type="number">
    ///   <item>Validate whether seeding is enabled.</item>
    ///   <item>Ensure the default analyst account exists.</item>
    ///   <item>Optionally seed demo data (clients, accounts, FX rates, rules).</item>
    /// </list>
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    public async Task SeedAsync(CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Database seeding disabled (Seed:Enabled=false).");
            return;
        }

        // Ensure default analyst account exists
        var analyst = await EnsureDefaultAnalystAsync(ct);
        await EnsureBaselineComplianceRulesAsync(ct);


        // Optionally seed demo data
        if (_options.SeedDemoData)
        {
            await EnsureDemoDataAsync(analyst, ct);
        }
        else
        {
            _logger.LogInformation("SeedDemoData disabled; only default analyst ensured.");
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Database seeding completed successfully.");
    }

    /// <summary>
    /// Ensures that the default analyst account exists and is up to date.
    /// </summary>
    /// <remarks>
    /// If the analyst already exists, their password and contact data are updated to match the current configuration.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ensured <see cref="Analyst"/> entity.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required default analyst configuration values are missing.
    /// </exception>
    private async Task<Analyst> EnsureDefaultAnalystAsync(CancellationToken ct)
    {
        var email = (_options.DefaultAnalyst.Email ?? string.Empty)
            .Trim()
            .ToLowerInvariant();

        var password = _options.DefaultAnalyst.Password ?? string.Empty;
        var fullName = _options.DefaultAnalyst.FullName ?? "Default Analyst";
        var phone = _options.DefaultAnalyst.PhoneNumber;

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException(
                "SeedOptions.DefaultAnalyst.Email is required when seeding is enabled."
            );

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "SeedOptions.DefaultAnalyst.Password is required when seeding is enabled."
            );

        var existing = await _db.Analysts
            .FirstOrDefaultAsync(a => a.CorporateEmail == email, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        if (existing is not null)
        {
            existing.SetPasswordHash(passwordHash);
            existing.UpdateContact(fullName, phone);

            _logger.LogInformation("Default analyst updated: {Email}", email);
            return existing;
        }

        var created = new Analyst(
            corporateEmail: email,
            passwordHash: passwordHash,
            fullName: fullName,
            phoneNumber: phone
        );

        _db.Analysts.Add(created);

        _logger.LogInformation("Created default analyst: {Email}", email);
        return created;
    }

    private async Task EnsureBaselineComplianceRulesAsync(CancellationToken ct)
    {
        // Create-if-missing by stable Code (never overwrite user-changed config).
        await EnsureRuleByCodeAsync(
            code: "daily_limit_default",
            create: () => new ComplianceRule(
                code: "daily_limit_default",
                ruleType: RuleType.DailyLimit,
                name: "Daily Limit (Default) - 10,000 USD",
                severity: Severity.Medium,
                parametersJson: JsonSerializer.Serialize(new
                {
                    limitBaseAmount = 10000.00m
                }),
                scope: "PerClient",
                isActive: true
            ),
            ct
        );

        await EnsureRuleByCodeAsync(
            code: "banned_countries_default",
            create: () => new ComplianceRule(
                code: "banned_countries_default",
                ruleType: RuleType.BannedCountries,
                name: "Banned Countries (Default)",
                severity: Severity.High,
                parametersJson: JsonSerializer.Serialize(new
                {
                    countries = new[] { "IR", "KP", "SY" }
                }),
                scope: null,
                isActive: true
            ),
            ct
        );

        await EnsureRuleByCodeAsync(
            code: "structuring_default",
            create: () => new ComplianceRule(
                code: "structuring_default",
                ruleType: RuleType.Structuring,
                name: "Structuring (Default) - 5 tx under 2,000 USD/day",
                severity: Severity.High,
                parametersJson: JsonSerializer.Serialize(new
                {
                    n = 5,
                    xBaseAmount = 2000.00m
                }),
                scope: "PerClient",
                isActive: true
            ),
            ct
        );

        await EnsureRuleByCodeAsync(
            code: "banned_accounts_default",
            create: () => new ComplianceRule(
                code: "banned_accounts_default",
                ruleType: RuleType.BannedAccounts,
                name: "Banned Accounts/Identifiers (Default)",
                severity: Severity.Critical,
                parametersJson: JsonSerializer.Serialize(new
                {
                    entries = Array.Empty<object>()
                }),
                scope: null,
                isActive: false
            ),
            ct
        );

        _logger.LogInformation("Baseline compliance rules ensured.");
    }

    private async Task EnsureRuleByCodeAsync(string code, Func<ComplianceRule> create, CancellationToken ct)
    {
        var exists = await _db.ComplianceRules.AsNoTracking()
            .AnyAsync(r => r.Code == code, ct);

        if (exists) return;

        _db.ComplianceRules.Add(create());
        _logger.LogInformation("Seeded compliance rule: {Code}", code);
    }


    /// <summary>
    /// Seeds optional demo data for development and testing environments.
    /// </summary>
    /// <remarks>
    /// Demo data includes:
    /// <list type="bullet">
    ///   <item>Clients</item>
    ///   <item>Accounts and identifiers</item>
    ///   <item>FX rates</item>
    ///   <item>Compliance rules</item>
    /// </list>
    /// <param name="analyst">The default analyst.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task EnsureDemoDataAsync(Analyst analyst, CancellationToken ct)
    {
        // ---- CLIENTS ----
        var mariaExists = await _db.Clients.AsNoTracking()
            .AnyAsync(c => c.Name == "Maria Santos Silva", ct);

        var abcExists = await _db.Clients.AsNoTracking()
            .AnyAsync(c => c.Name == "ABC Trading Corp", ct);

        var carlosExists = await _db.Clients.AsNoTracking()
            .AnyAsync(c => c.Name == "Carlos Rodriguez", ct);

        Client? clientBR = null;
        Client? clientUS = null;
        Client? clientAR = null;

        if (!mariaExists)
        {
            clientBR = new Client(
                LegalType.Individual,
                "Maria Santos Silva",
                "+5511987654321",
                JsonDocument.Parse(@"{
                    ""street"": ""Av Paulista, 1000"",
                    ""city"": ""São Paulo"",
                    ""state"": ""SP"",
                    ""zipCode"": ""01310-100""
                }"),
                "BR",
                RiskLevel.Low
            );
            clientBR.VerifyKyc();
            _db.Clients.Add(clientBR);
        }

        if (!abcExists)
        {
            clientUS = new Client(
                LegalType.Corporate,
                "ABC Trading Corp",
                "+12125551234",
                JsonDocument.Parse(@"{
                    ""street"": ""Wall Street, 100"",
                    ""city"": ""New York"",
                    ""state"": ""NY"",
                    ""zipCode"": ""10005""
                }"),
                "US",
                RiskLevel.Medium
            );
            _db.Clients.Add(clientUS);
        }

        if (!carlosExists)
        {
            clientAR = new Client(
                LegalType.Individual,
                "Carlos Rodriguez",
                "+541143211234",
                JsonDocument.Parse(@"{
                    ""street"": ""Av Corrientes, 500"",
                    ""city"": ""Buenos Aires"",
                    ""zipCode"": ""C1043""
                }"),
                "AR",
                RiskLevel.High
            );
            _db.Clients.Add(clientAR);
        }

        // ---- ACCOUNTS ----
        await EnsureAccountAsync("001-12345-6", () =>
        {
            var c = clientBR ?? _db.Clients.First(x => x.Name == "Maria Santos Silva");
            var a = new Account(c.Id, "001-12345-6", "BR", AccountType.Checking, "BRL");
            a.AddIdentifier(IdentifierType.CPF, "12345678900", "BR");
            a.AddIdentifier(IdentifierType.PIX_EMAIL, "maria@email.com", "BR");
            return a;
        }, ct);

        await EnsureAccountAsync("US-987654321", () =>
        {
            var c = clientUS ?? _db.Clients.First(x => x.Name == "ABC Trading Corp");
            var a = new Account(c.Id, "US-987654321", "US", AccountType.Investment, "USD");
            a.AddIdentifier(IdentifierType.LEI, "5493001KJTIIGC8Y1R12", "US");
            return a;
        }, ct);

        await EnsureAccountAsync("AR-543210", () =>
        {
            var c = clientAR ?? _db.Clients.First(x => x.Name == "Carlos Rodriguez");
            return new Account(c.Id, "AR-543210", "AR", AccountType.Savings, "ARS");
        }, ct);

        // ---- FX RATES ----
        await EnsureFxRateAsync("USD", "BRL", 5.25m, ct);
        await EnsureFxRateAsync("EUR", "BRL", 5.65m, ct);
    }

    /// <summary>
    /// Ensures an account exists based on its unique identifier.
    /// </summary>
    private async Task EnsureAccountAsync(
        string accountIdentifier,
        Func<Account> create,
        CancellationToken ct)
    {
        var exists = await _db.Accounts.AsNoTracking()
            .AnyAsync(a => a.AccountIdentifier == accountIdentifier, ct);

        if (exists) return;

        _db.Accounts.Add(create());
        _logger.LogInformation("Seeded account: {AccountNumber}", accountIdentifier);
    }

    /// <summary>
    /// Ensures a foreign exchange rate exists for the given currency pair.
    /// </summary>
    private async Task EnsureFxRateAsync(
        string baseCcy,
        string quoteCcy,
        decimal rate,
        CancellationToken ct)
    {
        var exists = await _db.FxRates.AsNoTracking()
            .AnyAsync(r => r.BaseCurrencyCode == baseCcy &&
                           r.QuoteCurrencyCode == quoteCcy, ct);

        if (exists) return;

        _db.FxRates.Add(new FxRate(
            baseCcy,
            quoteCcy,
            rate,
            DateTimeOffset.UtcNow.AddDays(-1)
        ));

        _logger.LogInformation("Seeded FX rate: {Base}/{Quote}", baseCcy, quoteCcy);
    }

    /// <summary>
    /// Ensures a compliance rule exists based on its name.
    /// </summary>
    private async Task EnsureComplianceRuleAsync(
        string name,
        Func<ComplianceRule> create,
        CancellationToken ct)
    {
        var exists = await _db.ComplianceRules.AsNoTracking()
            .AnyAsync(r => r.Name == name, ct);

        if (exists) return;

        _db.ComplianceRules.Add(create());
        _logger.LogInformation("Seeded compliance rule: {RuleName}", name);
    }
}
