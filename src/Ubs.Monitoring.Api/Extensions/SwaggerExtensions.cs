using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Ubs.Monitoring.Api.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Registers Swagger generation services with XML documentation and JWT authentication.
    /// </summary>
    /// <param name="services">The service collection to register Swagger services into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for registration.</returns>
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // API Info
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "UBS Monitoring API",
                Description = "API for monitoring, analyzing, and investigating financial transactions with focus on compliance, audit, traceability, and data quality.",
                Contact = new OpenApiContact
                {
                    Name = "UBS Monitoring Team",
                    Email = "support@ubsmonitoring.com"
                }
            });

            // Include XML comments
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your-token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

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
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "UBS Monitoring API v1");
            options.DocumentTitle = "UBS Monitoring API Documentation";
        });
        return app;
    }
}
