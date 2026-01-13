using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ubs.Monitoring.Application.Common;

namespace Ubs.Monitoring.Api.Extensions;

public sealed class CurrentRequestContext : ICurrentRequestContext
{
    private readonly IHttpContextAccessor _http;

    public CurrentRequestContext(IHttpContextAccessor http) => _http = http;

    public Guid? AnalystId
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx is null) return null;

            // Preferir JWT claim sub
            var sub = ctx.User.FindFirstValue("sub")
                   ?? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(sub, out var fromJwt))
                return fromJwt;

            // Fallback opcional: header (evite confiar nisso em produção)
            if (ctx.Request.Headers.TryGetValue("X-Analyst-Id", out var hdr) &&
                Guid.TryParse(hdr.ToString(), out var fromHeader))
                return fromHeader;

            return null;
        }
    }

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
