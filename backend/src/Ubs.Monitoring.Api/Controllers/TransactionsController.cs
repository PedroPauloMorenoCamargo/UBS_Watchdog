using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Infrastructure.Persistence;
namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
   

    public TransactionsController(
        AppDbContext context
        )
    {
        _context = context;
        
    }

    // GET /api/transactions/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction is null)
            return NotFound();

        return Ok(transaction);
    }

    
}
