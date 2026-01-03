using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ubs.Monitoring.Infrastructure.Persistence;


namespace Ubs.Monitoring.Api.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Liveness: always healthy
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            // Readiness: DB connectivity
            .AddDbContextCheck<AppDbContext>("db", tags: new[] { "ready" });

        return services;
    }

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

            // Liveness: usually always 200
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

            // Readiness: 200 if healthy, 503 otherwise
            var code = report.Status == HealthStatus.Healthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;

            return Results.Json(payload, statusCode: code);
        })
        .WithTags("Health")
        .AllowAnonymous()
        .WithOpenApi();

        return app;
    }
}
