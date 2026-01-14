using FluentValidation;
using Ubs.Monitoring.Api.Filters;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common;
namespace Ubs.Monitoring.Api.Extensions;

public static class ApiServiceExtensions
{
    /// <summary>
    /// Registers core API services, including controllers, endpoint exploration, and Problem Details support.</summary>
    /// <param name="services">The service collection to register API services into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for t registration.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Global validation filter - automatically validates all requests
            options.Filters.Add<ValidationFilter>();
        });

        services.AddEndpointsApiExplorer();

        // RFC7807 / application/problem+json
        services.AddProblemDetails();

        // FluentValidation - registers all validators from Application assembly
        services.AddValidatorsFromAssemblyContaining<CreateClientRequest>();
        // Saber quem faz a request
        services.AddScoped<ICurrentRequestContext, CurrentRequestContext>();


        return services;
    }
}
