using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ubs.Monitoring.Api.Extensions;
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog as the application's logging provider, reading configuration from the host configuration system.
    /// </summary>
    /// <param name="host">The host builder to configure.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same <see cref="IHostBuilder"/> instance for fluent configuration.</returns>
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host, IConfiguration configuration)
    {
        host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
        return host;
    }
}
