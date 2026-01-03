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
        app.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live"),
            ResponseWriter = WriteJson
        }).AllowAnonymous();

        app.MapHealthChecks("/api/health/db", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready"),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = WriteJson
        }).AllowAnonymous();

        return app;
    }

    private static Task WriteJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

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

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
