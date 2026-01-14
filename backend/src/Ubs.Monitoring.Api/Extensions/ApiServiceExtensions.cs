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
    /// Registers core API services, validation, and cross-cutting concerns.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Centralized request validation -> RFC7807
            options.Filters.Add<ValidationFilter>();
        });

        services.AddEndpointsApiExplorer();
        services.AddProblemDetails();

        // FluentValidation (automatic MVC integration)
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // Scan Application validators
        services.AddValidatorsFromAssemblyContaining<CreateClientRequest>();

        // Scan API validators (Auth, Analysts, etc.)
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        // Request context (JWT, correlation id, etc.)
        services.AddScoped<ICurrentRequestContext, CurrentRequestContext>();

        return services;
    }
}
