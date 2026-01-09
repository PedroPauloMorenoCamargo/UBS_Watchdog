namespace Ubs.Monitoring.Application.Common.FileExport;

/// <summary>
/// Supported export file formats.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// CSV format (comma-separated values).
    /// Can be opened in Excel, Google Sheets, etc.
    /// </summary>
    Csv,
    
    /// <summary>
    /// PDF format (formatted report with styling).
    /// </summary>
    Pdf
}
