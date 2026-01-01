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

    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
    }

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
