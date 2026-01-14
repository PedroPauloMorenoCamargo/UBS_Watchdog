using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ubs.Monitoring.Application.Common;

namespace Ubs.Monitoring.Api.Extensions;

public sealed class CurrentRequestContext : ICurrentRequestContext
{
    private readonly IHttpContextAccessor _http;

    public CurrentRequestContext(IHttpContextAccessor http) => _http = http;

    /// <summary>
    /// Gets the identifier of the authenticated analyst associated with the current request.
    /// </summary>
    /// <returns>
    /// The analyst identifier extracted from the JWT <c>sub</c> or <see cref="ClaimTypes.NameIdentifier"/> claim, or from the <c>X-Analyst-Id</c> 
    /// request header when present; otherwise, <c>null</c> if no valid identifier can be resolved.
    /// </returns>
    public Guid? AnalystId
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return null;

            //  JWT claim sub
            var sub = ctx.User.FindFirstValue("sub")
                   ?? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(sub, out var fromJwt))
                return fromJwt;

            if (ctx.Request.Headers.TryGetValue("X-Analyst-Id", out var hdr) &&
                Guid.TryParse(hdr.ToString(), out var fromHeader))
                return fromHeader;

            return null;
        }
    }
    /// <summary>
    /// Gets the correlation identifier associated with the current request.
    /// </summary>
    /// <returns>
    /// The value of the <c>X-Correlation-Id</c> request header when provided; otherwise, the ASP.NET Core generated trace identifier, or <c>null</c>
    /// if no HTTP context is available.
    /// </returns>
    public string? CorrelationId
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return null;

            if (ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var hdr) &&
                !string.IsNullOrWhiteSpace(hdr))
                return hdr.ToString();

            return ctx.TraceIdentifier;
        }
    }
}
