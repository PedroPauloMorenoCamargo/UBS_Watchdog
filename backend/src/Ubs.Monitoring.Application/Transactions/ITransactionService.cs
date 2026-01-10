namespace Ubs.Monitoring.Application.Transactions;

public interface ITransactionService
{
    Task<Guid> CreateAsync(CreateTransactionRequest request, CancellationToken ct);
    Task<IReadOnlyList<TransactionResponse>> GetByClientAsync(Guid clientId, CancellationToken ct);
}
