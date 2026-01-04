namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Service interface for parsing client import files (CSV and Excel).
/// </summary>
public interface IClientFileImportService
{
    /// <summary>
    /// Parses a CSV or Excel file and returns a list of client import rows.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The file name (used to determine file type).</param>
    /// <returns>List of parsed client rows.</returns>
    /// <exception cref="InvalidOperationException">Thrown when file format is invalid or unsupported.</exception>
    List<ClientImportRow> ParseFile(Stream stream, string fileName);
}
