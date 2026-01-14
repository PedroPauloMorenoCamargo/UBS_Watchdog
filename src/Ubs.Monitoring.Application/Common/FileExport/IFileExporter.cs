namespace Ubs.Monitoring.Application.Common.FileExport;

/// <summary>
/// Generic interface for exporting data to files (CSV, PDF, Excel).
/// Each report type implements its own exporter with specific formatting logic.
/// </summary>
/// <typeparam name="TData">The data type to export.</typeparam>
public interface IFileExporter<in TData> where TData : class
{
    /// <summary>
    /// Exports data to a file in the specified format.
    /// </summary>
    /// <param name="data">Data to export.</param>
    /// <param name="format">Export format (CSV, PDF, Excel).</param>
    /// <param name="options">Optional export options (headers, styling, etc.).</param>
    /// <returns>File content as byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown when format is unsupported or export fails.</exception>
    Task<byte[]> ExportAsync(TData data, ExportFormat format, ExportOptions? options = null);
}

/// <summary>
/// Options for customizing export behavior.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Report title (used in PDF/Excel).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Include timestamp in the report.
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Custom headers for columns (if different from default).
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }

    /// <summary>
    /// Date format for date fields.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Currency format for monetary values.
    /// </summary>
    public string CurrencyFormat { get; set; } = "N2";
}
