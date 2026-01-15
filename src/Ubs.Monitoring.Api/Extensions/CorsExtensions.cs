using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ubs.Monitoring.Api.Extensions;

public static class CorsExtensions
{
    private const string FrontendPolicyName = "frontend";

    /// <summary>
    /// Registers the frontend CORS policy. If <c>Cors:FrontendOrigin</c> is not configured, defaults to <c>https://ubs-watchdog-gamma.vercel.app</c>.
    /// </summary>
    /// <param name="services">The service collection to register CORS services into.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent registration.</returns>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origin = configuration["Cors:FrontendOrigin"] ?? "https://ubs-watchdog-gamma.vercel.app";

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicyName, policy =>
                policy.WithOrigins(origin)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials());
        });

        return services;
    }

    /// <summary>
    /// Applies the frontend CORS policy to the request pipeline.
    /// </summary>
    /// <param name="app">The application pipeline builder.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for fluent configuration.</returns>
    public static WebApplication UseFrontendCors(this WebApplication app)
    {
        app.UseCors(FrontendPolicyName);
        return app;
    }
}
