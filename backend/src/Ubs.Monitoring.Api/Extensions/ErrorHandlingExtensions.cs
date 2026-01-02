using Microsoft.AspNetCore.Builder;

namespace Ubs.Monitoring.Api.Extensions;

public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Adds centralized exception handling and status code page middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application pipeline builder.</param>
    /// <returns>The same <see cref="WebApplication"/> instance for configuration.</returns>
    public static WebApplication UseApiErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}
