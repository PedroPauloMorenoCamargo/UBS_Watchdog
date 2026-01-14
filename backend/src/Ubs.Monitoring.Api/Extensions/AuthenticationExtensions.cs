using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ubs.Monitoring.Api.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers JWT Bearer authentication for HTTP APIs and SignalR hubs.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
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

                // --------------------------------------------------------
                //  REQUIRED FOR SIGNALR (WebSockets)
                // --------------------------------------------------------
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // SignalR sends token via query string
                        var accessToken =
                            context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        // Only allow this for SignalR hubs
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
