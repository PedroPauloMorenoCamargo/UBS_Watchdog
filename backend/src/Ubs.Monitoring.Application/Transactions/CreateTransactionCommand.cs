using MediatR;

namespace Ubs.Monitoring.Application.Transactions.Create;

public record CreateTransactionCommand(
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Description
) : IRequest<Guid>;
