using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ubs.Monitoring.Application.AccountIdentifiers;
using Ubs.Monitoring.Application.Accounts;
using Ubs.Monitoring.Application.Analysts;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common.FileImport;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Application.Countries;
using Ubs.Monitoring.Application.FxRates;
using Ubs.Monitoring.Application.Cases;
using Ubs.Monitoring.Application.Transactions;
using Ubs.Monitoring.Application.Reports;
using Ubs.Monitoring.Infrastructure.Reports;
using Ubs.Monitoring.Application.Transactions.Repositories;
using Ubs.Monitoring.Infrastructure.Auth;
using Ubs.Monitoring.Infrastructure.ExternalServices;
using Ubs.Monitoring.Infrastructure.Persistence;
using Ubs.Monitoring.Infrastructure.Persistence.Seeding;
using Ubs.Monitoring.Infrastructure.Repositories;
using Ubs.Monitoring.Application.Transactions.Compliance;
namespace Ubs.Monitoring.Infrastructure;
using Ubs.Monitoring.Application.AuditLogs;
using Ubs.Monitoring.Infrastructure.Persistence.Repositories;
using Ubs.Monitoring.Infrastructure.Persistence.Auditing;

public static class DependencyInjection
{   
    /// <summary>
    /// Registers all infrastructure services: persistence, authentication, repositories, etc.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs =
            config["ConnectionStrings:Default"]
            ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((sp,options) =>
        {
            options.UseNpgsql(cs, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });

            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });
        // Seed
        services.Configure<SeedOptions>(config.GetSection("Seed"));
        services.AddScoped<DatabaseSeeder>();
        // Auth
        services.AddOptions<JwtOptions>().Bind(config.GetSection("Jwt")).ValidateOnStart();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        // Analysts
        services.AddScoped<IAnalystRepository, AnalystRepository>();
        services.AddScoped<IAuthService, AuthService>();
        // Analysts Profile
        services.AddScoped<IAnalystProfileRepository, AnalystProfileRepository>();
        services.AddScoped<IAnalystProfileService, AnalystProfileService>();
        // Compliance Rules
        services.AddScoped<IComplianceRuleRepository, ComplianceRuleRepository>();
        services.AddScoped<IComplianceRuleParametersValidator, ComplianceRuleParametersValidator>();
        services.AddScoped<IComplianceRuleService, ComplianceRuleService>();
        services.AddScoped<ITransactionComplianceChecker, TransactionComplianceChecker>();
        // Clients
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IFileParser<ClientImportRow>, ClientFileImportService>();
        services.AddScoped<IClientService, ClientService>();

        // Countries
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ICountryService, CountryService>();
        // Accounts
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IFileParser<AccountImportRow>, AccountFileImportService>();
        services.AddScoped<IAccountService, AccountService>();
        // Account Identifiers
        services.AddScoped<IAccountIdentifierService, AccountIdentifierService>();
        // FxRates
        services.AddScoped<IFxRateRepository, FxRateRepository>();
        // Exchange Rate Provider (External API)
        services.AddExchangeRateApiProvider(config);
        // FxRate Service (currency conversion orchestration)
        services.AddOptions<FxRateServiceOptions>()
            .Bind(config.GetSection("FxRateService"))
            .ValidateOnStart();
        services.AddScoped<IFxRateService, FxRateService>();
        // Transactions
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFileParser<TransactionImportRow>, TransactionFileImportService>();
        services.AddScoped<ITransactionService, TransactionService>();
        // Cases
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<ICaseService, CaseService>();
        // Reports
        services.AddScoped<IReportService, ReportService>();
        // Logs
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        
        return services;
    }

    /// <summary>
    /// Registers ExchangeRate-API provider with HttpClient, caching, and retry.
    /// </summary>
    private static IServiceCollection AddExchangeRateApiProvider(this IServiceCollection services, IConfiguration config)
    {
        // Bind configuration
        services.AddOptions<ExchangeRateApiOptions>()
            .Bind(config.GetSection(ExchangeRateApiOptions.SectionName))
            .PostConfigure(options =>
            {
                // Allow environment variable override for API key
                var envApiKey = Environment.GetEnvironmentVariable("EXCHANGERATE_API_KEY");
                if (!string.IsNullOrWhiteSpace(envApiKey))
                {
                    options.ApiKey = envApiKey;
                }
            })
            .ValidateOnStart();

        // Add memory cache for exchange rate caching
        services.AddMemoryCache();

        // Configure HttpClient with timeout
        var exchangeRateOptions = config.GetSection(ExchangeRateApiOptions.SectionName).Get<ExchangeRateApiOptions>()
            ?? new ExchangeRateApiOptions();

        services.AddHttpClient<IExchangeRateProvider, ExchangeRateApiProvider>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(exchangeRateOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "UBS.Monitoring/1.0");
        });

        return services;
    }
}
