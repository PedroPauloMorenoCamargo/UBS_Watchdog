using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ubs.Monitoring.Infrastructure.Persistence;
using Ubs.Monitoring.Infrastructure.Persistence.Seeding;

namespace Ubs.Monitoring.Api.Startup;
public static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Applies EF Core migrations and executes database seeding with retry semantics. Intended for development environments.
    /// </summary>
    /// <param name="app">The running web application.</param>
    /// <returns>A task that completes when initialization succeeds.</returns>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await ApplyMigrationsWithRetryAsync(app);
        await SeedWithRetryAsync(app);
    }

    private static async Task ApplyMigrationsWithRetryAsync(WebApplication app)
    {
        await RetryHelper.ExecuteAsync(async ct =>
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(ct);
        });
    }

    private static async Task SeedWithRetryAsync(WebApplication app)
    {
        await RetryHelper.ExecuteAsync(async ct =>
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync(ct);
        });
    }
}
