namespace Ubs.Monitoring.Application.Transactions;

/// <summary>
/// Constants used by the transaction service for import processing.
/// </summary>
public static class TransactionServiceConstants
{
    /// <summary>
    /// Number of transactions to process before saving to database during import.
    /// </summary>
    public const int ImportBatchSize = 500;

    /// <summary>
    /// Line number offset for import error reporting.
    /// Accounts for header row (line 1) and 0-based index.
    /// </summary>
    public const int ImportLineNumberOffset = 2;
}
