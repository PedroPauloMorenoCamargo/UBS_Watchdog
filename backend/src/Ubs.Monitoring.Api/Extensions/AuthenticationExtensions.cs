using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ubs.Monitoring.Api.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication for the API, including support for HTTP requests and SignalR connections.
    /// </summary>
    /// <param name="services">
    /// The dependency injection container used to register authentication services.
    /// </param>
    /// <param name="configuration">
    /// The application configuration source used to read JWT settings such as issuer, audience, and signing key.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance, enabling fluent  registration of additional services.
    /// </returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var signingKey = jwtSection["SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException("Jwt:SigningKey is missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Usual  JWT
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                // SignalR JWT
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken =
                            context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
