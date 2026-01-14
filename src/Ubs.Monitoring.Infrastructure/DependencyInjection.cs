using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Ubs.Monitoring.Infrastructure.Persistence;
using Ubs.Monitoring.Infrastructure.Persistence.Seeding;
using Ubs.Monitoring.Infrastructure.Persistence.Auditing;
using Ubs.Monitoring.Infrastructure.Repositories;
using Ubs.Monitoring.Infrastructure.Auth;
using Ubs.Monitoring.Infrastructure.ExternalServices;
using Ubs.Monitoring.Infrastructure.Reports;
using Ubs.Monitoring.Application.AuditLogs;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Application.Analysts;
using Ubs.Monitoring.Application.Accounts;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Countries;
using Ubs.Monitoring.Application.ComplianceRules;
using Ubs.Monitoring.Application.FxRates;
using Ubs.Monitoring.Application.Cases;
using Ubs.Monitoring.Application.Transactions;
using Ubs.Monitoring.Application.Reports;

namespace Ubs.Monitoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Resolve connection string (Railway-safe)
        var raw = configuration.GetConnectionString("Default")
                  ?? configuration["DATABASE_URL"]
                  ?? throw new InvalidOperationException("Database connection string is missing.");

        var connectionString = raw.StartsWith("postgres", StringComparison.OrdinalIgnoreCase)
            ? new NpgsqlConnectionStringBuilder(raw).ConnectionString
            : raw;

        // Audit interceptor
        services.AddScoped<AuditSaveChangesInterceptor>();

        // DbContext (registered ONCE)
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });

            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // Seed
        services.Configure<SeedOptions>(configuration.GetSection("Seed"));
        services.AddScoped<DatabaseSeeder>();

        // Auth
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateOnStart();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // Repositories & services
        services.AddScoped<IAnalystRepository, AnalystRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAnalystProfileRepository, AnalystProfileRepository>();
        services.AddScoped<IAnalystProfileService, AnalystProfileService>();
        services.AddScoped<IComplianceRuleRepository, ComplianceRuleRepository>();
        services.AddScoped<IComplianceRuleService, ComplianceRuleService>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // External services
        services.AddExchangeRateApiProvider(configuration);

        return services;
    }

    private static IServiceCollection AddExchangeRateApiProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ExchangeRateApiOptions>()
            .Bind(configuration.GetSection(ExchangeRateApiOptions.SectionName))
            .ValidateOnStart();

        services.AddMemoryCache();

        services.AddHttpClient<IExchangeRateProvider, ExchangeRateApiProvider>();

        return services;
    }
}
