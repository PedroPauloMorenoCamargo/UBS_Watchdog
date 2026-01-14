namespace Ubs.Monitoring.Application.Common.FileImport;

/// <summary>
/// Result of a file import operation.
/// </summary>
/// <param name="TotalRows">Total rows processed.</param>
/// <param name="SuccessCount">Number of successfully imported rows.</param>
/// <param name="ErrorCount">Number of rows with errors.</param>
/// <param name="Errors">List of error messages (with row numbers).</param>
public record ImportResult(
    int TotalRows,
    int SuccessCount,
    int ErrorCount,
    List<string> Errors
)
{
    /// <summary>
    /// Creates a successful import result.
    /// </summary>
    public static ImportResult Success(int successCount) => new(
        TotalRows: successCount,
        SuccessCount: successCount,
        ErrorCount: 0,
        Errors: new List<string>()
    );

    /// <summary>
    /// Creates an import result with partial success.
    /// </summary>
    public static ImportResult Partial(int totalRows, int successCount, List<string> errors) => new(
        TotalRows: totalRows,
        SuccessCount: successCount,
        ErrorCount: errors.Count,
        Errors: errors
    );
}
