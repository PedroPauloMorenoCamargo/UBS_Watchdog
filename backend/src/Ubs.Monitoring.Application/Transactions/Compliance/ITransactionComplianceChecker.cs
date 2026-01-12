using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Transactions.Compliance;

public interface ITransactionComplianceChecker
{
    /// <summary>
    /// Evaluates the transaction against active compliance rules and logs any violations found.
    /// </summary>
    Task CheckAsync(Transaction transaction, CancellationToken ct);
}
