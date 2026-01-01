using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;

    public DatabaseSeeder(AppDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        // NOTE: Simple idempotency check adequate for development.
        // All data is saved in a single SaveChangesAsync() to ensure atomicity.
        // If seeding fails, EF Core's implicit transaction will rollback everything.
        if (await _db.Analysts.AnyAsync())
            return;

        // 1. ANALYST
        var admin = new Analyst(
            corporateEmail: "admin@ubs.com",
            passwordHash: "admin123",
            fullName: "Admin Sistema UBS",
            phoneNumber: "+5511999999999"
        );
        _db.Analysts.Add(admin);

        // 2. CLIENTS
        var clientBR = new Client(
            legalType: LegalType.Individual,
            name: "Maria Santos Silva",
            contactNumber: "+5511987654321",
            addressJson: JsonDocument.Parse(@"{
                ""street"": ""Av Paulista, 1000"",
                ""city"": ""São Paulo"",
                ""state"": ""SP"",
                ""zipCode"": ""01310-100""
            }"),
            countryCode: "BR",
            initialRiskLevel: RiskLevel.Low
        );
        clientBR.VerifyKyc(); 

        var clientUS = new Client(
            legalType: LegalType.Corporate,
            name: "ABC Trading Corp",
            contactNumber: "+12125551234",
            addressJson: JsonDocument.Parse(@"{
                ""street"": ""Wall Street, 100"",
                ""city"": ""New York"",
                ""state"": ""NY"",
                ""zipCode"": ""10005""
            }"),
            countryCode: "US",
            initialRiskLevel: RiskLevel.Medium
        );

        var clientAR = new Client(
            legalType: LegalType.Individual,
            name: "Carlos Rodriguez",
            contactNumber: "+541143211234",
            addressJson: JsonDocument.Parse(@"{
                ""street"": ""Av Corrientes, 500"",
                ""city"": ""Buenos Aires"",
                ""zipCode"": ""C1043""
            }"),
            countryCode: "AR",
            initialRiskLevel: RiskLevel.High
        );

        _db.Clients.AddRange(clientBR, clientUS, clientAR);

        // 3. ACCOUNTS

        var accountBR = new Account(
            clientBR.Id,
            "001-12345-6",
            "BR",
            AccountType.Checking,
            "BRL"
        );
        accountBR.AddIdentifier(IdentifierType.CPF, "12345678900", "BR");
        accountBR.AddIdentifier(IdentifierType.PIX_EMAIL, "maria@email.com", "BR");

        var accountUS = new Account(
            clientUS.Id,
            "US-987654321",
            "US",
            AccountType.Investment,
            "USD"
        );
        accountUS.AddIdentifier(IdentifierType.LEI, "5493001KJTIIGC8Y1R12", "US");

        var accountAR = new Account(
            clientAR.Id,
            "AR-543210",
            "AR",
            AccountType.Savings,
            "ARS"
        );

        _db.Accounts.AddRange(accountBR, accountUS, accountAR);

        // 4. FX_RATES
        var fxUsdBrl = new FxRate(
            baseCurrencyCode: "USD",
            quoteCurrencyCode: "BRL",
            rate: 5.25m,
            asOfUtc: DateTimeOffset.UtcNow.AddDays(-1)
        );

        var fxEurBrl = new FxRate(
            baseCurrencyCode: "EUR",
            quoteCurrencyCode: "BRL",
            rate: 5.65m,
            asOfUtc: DateTimeOffset.UtcNow.AddDays(-1)
        );

        _db.FxRates.AddRange(fxUsdBrl, fxEurBrl);

        // 5. COMPLIANCE_RULES
        var ruleDailyLimit = new ComplianceRule(
            ruleType: RuleType.DailyLimit,
            name: "Limite Diário - R$ 10.000",
            severity: Severity.Medium,
            parametersJson: JsonDocument.Parse(@"{
                ""limitBaseAmount"": 10000,
                ""currencyCode"": ""BRL""
            }"),
            scope: "BR",
            isActive: true
        );

        var ruleBannedCountries = new ComplianceRule(
            ruleType: RuleType.BannedCountries,
            name: "Países de Alto Risco",
            severity: Severity.High,
            parametersJson: JsonDocument.Parse(@"{
                ""bannedCountryCodes"": [""IR"", ""KP"", ""SY""]
            }"),
            scope: null, 
            isActive: true
        );

        _db.ComplianceRules.AddRange(ruleDailyLimit, ruleBannedCountries);

        // 6. ATOMIC SAVE - All entities saved in a single transaction
        await _db.SaveChangesAsync();
        Console.WriteLine("✅ Database seeded successfully: 1 Analyst, 3 Clients, 3 Accounts, 2 FxRates, 2 ComplianceRules");
    }
}
