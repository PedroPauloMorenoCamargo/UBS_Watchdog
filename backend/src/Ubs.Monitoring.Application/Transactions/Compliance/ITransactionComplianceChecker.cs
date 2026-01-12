using Ubs.Monitoring.Domain.Entities;

namespace Ubs.Monitoring.Application.Transactions.Compliance;

public interface ITransactionComplianceChecker
{
    /// <summary>
    /// Evaluates the transaction against active compliance rules and returns any violations found.
    /// Creates cases and case findings automatically if violations are detected.
    /// </summary>
    Task CheckAndCreateCaseIfNeededAsync(Transaction transaction, CancellationToken ct);
}
