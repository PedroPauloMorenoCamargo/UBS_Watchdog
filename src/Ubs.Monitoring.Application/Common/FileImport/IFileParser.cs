namespace Ubs.Monitoring.Application.Common.FileImport;

/// <summary>
/// Generic interface for parsing files (CSV/Excel) into specific entity import models.
/// Each entity implements its own parser with specific field mapping logic.
/// </summary>
/// <typeparam name="TImportRow">The entity-specific import row model.</typeparam>
public interface IFileParser<TImportRow> where TImportRow : class
{
    /// <summary>
    /// Parses a file stream and returns a list of entity import rows.
    /// </summary>
    /// <param name="stream">File stream to parse.</param>
    /// <param name="fileName">File name (used to determine format - CSV or Excel).</param>
    /// <returns>List of parsed import rows.</returns>
    /// <exception cref="InvalidOperationException">Thrown when file format is invalid or parsing fails.</exception>
    List<TImportRow> ParseFile(Stream stream, string fileName);
}
