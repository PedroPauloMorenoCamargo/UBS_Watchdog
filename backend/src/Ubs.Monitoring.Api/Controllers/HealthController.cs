using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Infrastructure.Persistence;

namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Basic liveness probe.
    /// </summary>
    /// <remarks>
    /// This endpoint verifies that the application process is running and capable of handling HTTP requests.
    /// </remarks>
    /// <returns>
    /// HTTP 200 with a simple status payload when the service is alive.
    /// </returns>
    /// <response code="200">Service is alive.</response>
    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
    }

    /// <summary>
    /// Database readiness probe.
    /// </summary>
    /// <remarks>
    /// This endpoint verifies whether the application can establish  a connection to the configured database.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// HTTP 200 if the database is reachable; otherwise HTTP 503.
    /// </returns>
    /// <response code="200">Database connection is healthy.</response>
    /// <response code="503">Database is unavailable.</response>
    [HttpGet("db")]
    public async Task<IActionResult> Database(CancellationToken ct)
    {
        var canConnect = await _db.Database.CanConnectAsync(ct);

        if (!canConnect)
        {
            return Problem(
                title: "Database unavailable",
                detail: "Cannot connect to the database",
                statusCode: StatusCodes.Status503ServiceUnavailable
            );
        }

        return Ok(new { db = "up" });
    }
}
