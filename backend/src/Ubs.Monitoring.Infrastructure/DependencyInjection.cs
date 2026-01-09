using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ubs.Monitoring.Application.Analysts;
using Ubs.Monitoring.Application.Auth;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Infrastructure.Auth;
using Ubs.Monitoring.Infrastructure.Persistence;
using Ubs.Monitoring.Infrastructure.Persistence.Repositories;
using Ubs.Monitoring.Infrastructure.Persistence.Seeding;
using Ubs.Monitoring.Application.ComplianceRules;


namespace Ubs.Monitoring.Infrastructure;

public static class DependencyInjection
{   
    /// <summary>
    /// Registers all infrastructure services, including persistence, authentication, application support services, etc.
    /// </summary>
    /// <param name="services">
    /// The service collection to which infrastructure services are added.
    /// </param>
    /// <param name="config">
    /// The application configuration used to bind infrastructure settings.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance to allow fluent service registration.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the default database connection string is missing from the configuration.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:Default is missing.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(cs, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
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
        services.AddSingleton<IComplianceRuleParametersValidator, ComplianceRuleParametersValidator>();
        services.AddScoped<IComplianceRuleService, ComplianceRuleService>();
        // Clients
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IClientFileImportService, ClientFileImportService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITransactionService, TransactionService>();


        return services;
    }
}
