using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ubs.Monitoring.Infrastructure.Persistence;


namespace Ubs.Monitoring.Api.Extensions;

public static class HealthChecksExtensions
{
    /// <summary>
    /// Registers health check services used by the API to report liveness and readiness status.
    /// </summary>
    /// <param name="services">
    /// The dependency injection container used to register health check implementations.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance, allowing fluent  chaining of service registrations.
    /// </returns>
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddDbContextCheck<AppDbContext>("db", tags: new[] { "ready" });

        return services;
    }
    /// <summary>
    /// Maps HTTP endpoints that expose health check information for the API.
    /// </summary>
    /// <param name="app">
    /// The <see cref="WebApplication"/> instance used to configure endpoint routing.
    /// </param>
    /// <returns>
    /// The same <see cref="WebApplication"/> instance, enabling fluent chaining of endpoint mappings.
    /// </returns>
   public static WebApplication MapApiHealthChecks(this WebApplication app)
    {
        app.MapGet("/api/health", async (HealthCheckService hc, CancellationToken ct) =>
        {
            var report = await hc.CheckHealthAsync(r => r.Tags.Contains("live"), ct);

            var payload = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            };
            return Results.Json(payload, statusCode: StatusCodes.Status200OK);
        })
        .WithTags("Health")
        .AllowAnonymous()
        .WithOpenApi();

        app.MapGet("/api/health/db", async (HealthCheckService hc, CancellationToken ct) =>
        {
            var report = await hc.CheckHealthAsync(r => r.Tags.Contains("ready"), ct);

            var payload = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            };

            var code = report.Status == HealthStatus.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

            return Results.Json(payload, statusCode: code);
        })
        .WithTags("Health")
        .AllowAnonymous()
        .WithOpenApi();

        return app;
    }
}
