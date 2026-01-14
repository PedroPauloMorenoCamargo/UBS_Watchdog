using Microsoft.AspNetCore.Builder;

namespace Ubs.Monitoring.Api.Extensions;

public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Adds exception handling and status code page middleware.
    /// </summary>
    public static WebApplication UseApiErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}
