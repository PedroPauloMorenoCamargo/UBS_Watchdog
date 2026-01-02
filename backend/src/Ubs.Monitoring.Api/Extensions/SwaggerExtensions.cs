using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ubs.Monitoring.Api.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Registers Swagger generation services.
    /// </summary>
    /// <param name="services">The service collection to register Swagger services into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for registration.</returns>
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        return services;
    }

    /// <summary>
    /// Adds Swagger middleware to the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application pipeline builder.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for fluent configuration.</returns>
    public static WebApplication UseSwaggerPipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
