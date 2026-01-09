using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Transactions.Create;
using Ubs.Monitoring.Infrastructure.Persistence;
using Ubs.Monitoring.Application.Transactions.Scheduling;


namespace Ubs.Monitoring.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _context;
    private readonly ITransactionSchedulerService _schedulerService;


    public TransactionsController(IMediator mediator,AppDbContext context,ITransactionSchedulerService schedulerService)
{
    _mediator = mediator;
    _context = context;
    _schedulerService = schedulerService;
}


    // POST /api/transactions
    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
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

        // POST /api/transactions/schedule
    [HttpPost("schedule")]
    public async Task<IActionResult> Schedule(
        [FromBody] ScheduleTransactionRequest request,
        CancellationToken ct)
    {
        var id = await _schedulerService.ScheduleAsync(
            request.AccountId,
            request.Amount,
            request.Currency,
            request.ScheduledForUtc,
            ct);

        return Ok(new { ScheduledTransactionId = id });
    }

}
