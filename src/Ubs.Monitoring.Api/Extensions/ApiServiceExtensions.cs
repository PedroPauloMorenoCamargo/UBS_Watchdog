using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Ubs.Monitoring.Api.Filters;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common;

namespace Ubs.Monitoring.Api.Extensions;
public static class ApiServiceExtensions
{
    /// <summary>
    /// Registers API-layer services .
    /// </summary>
    /// <param name="services">
    /// The dependency injection container used to register controllers, validation, request context services, and API infrastructure components.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance, allowing fluent chaining of additional service registrations.
    /// </returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Applies centralized request validation and maps validation errors to RFC 7807 (Problem Details) responses.
            options.Filters.Add<ValidationFilter>();
        });

        // Enables endpoint metadata discovery for tools such as Swagger
        services.AddEndpointsApiExplorer();

        // Enables standardized RFC 7807 Problem Details responses
        services.AddProblemDetails();
        
        // Registers FluentValidation validators from the Application layer
        services.AddValidatorsFromAssemblyContaining<CreateClientRequest>();

        // Registers FluentValidation validators from the API layer
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        // Registers a scoped service that exposes request-scoped context such as JWT claims and correlation identifiers
        services.AddScoped<ICurrentRequestContext, CurrentRequestContext>();

        return services;
    }
}

