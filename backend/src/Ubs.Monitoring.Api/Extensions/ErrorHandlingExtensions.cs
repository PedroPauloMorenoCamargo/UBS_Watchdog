using Microsoft.AspNetCore.Builder;

namespace Ubs.Monitoring.Api.Extensions;

public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Configures global exception handling and HTTP status code handling for the API request pipeline.
    /// </summary>
    /// <param name="app">
    /// The <see cref="WebApplication"/> instance used to configure the HTTP request pipeline.
    /// </param>
    /// <returns>
    /// The same <see cref="WebApplication"/> instance, allowing fluent chaining of middleware configuration calls.
    /// </returns>
    public static WebApplication UseApiErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}
