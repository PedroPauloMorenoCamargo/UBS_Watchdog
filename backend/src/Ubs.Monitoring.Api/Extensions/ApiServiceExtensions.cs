using Microsoft.Extensions.DependencyInjection;

namespace Ubs.Monitoring.Api.Extensions;

public static class ApiServiceExtensions
{
    /// <summary>
    /// Registers core API services, including controllers, endpoint exploration, and Problem Details support.</summary>
    /// <param name="services">The service collection to register API services into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for t registration.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // RFC7807 / application/problem+json
        services.AddProblemDetails();

        return services;
    }
}
