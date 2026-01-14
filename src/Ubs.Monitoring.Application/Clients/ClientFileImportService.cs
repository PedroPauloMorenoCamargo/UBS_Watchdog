using Ubs.Monitoring.Application.Common.FileImport;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Client-specific implementation of file parser.
/// Handles CSV and Excel parsing with client-specific field mapping.
/// </summary>
public sealed class ClientFileImportService : IFileParser<ClientImportRow>
{
    /// <summary>
    /// Parses a CSV or Excel file and returns a list of client import rows.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The file name (used to determine file type).</param>
    /// <returns>List of parsed client rows.</returns>
    /// <exception cref="InvalidOperationException">Thrown when file format is invalid or unsupported.</exception>
    public List<ClientImportRow> ParseFile(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".csv" => ParseCsv(stream),
            ".xlsx" or ".xls" => ParseExcel(stream),
            _ => throw new InvalidOperationException(
                $"Unsupported file format: {extension}. " +
                $"Supported formats: {string.Join(", ", FileParsingHelper.SupportedExtensions)}")
        };
    }

    /// <summary>
    /// Parses a CSV file using generic helper.
    /// </summary>
    private List<ClientImportRow> ParseCsv(Stream stream)
    {
        return FileParsingHelper.ParseCsv<ClientImportRow>(stream);
    }

    /// <summary>
    /// Parses an Excel file using generic helper with client-specific mapping.
    /// </summary>
    private List<ClientImportRow> ParseExcel(Stream stream)
    {
        var rawRows = FileParsingHelper.ParseExcelRaw(stream, worksheetIndex: 1);
        var clients = new List<ClientImportRow>();

        foreach (var row in rawRows)
        {
            var client = new ClientImportRow
            {
                LegalType = FileParsingHelper.GetColumnValue(row, "LegalType"),
                Name = FileParsingHelper.GetColumnValue(row, "Name"),
                ContactNumber = FileParsingHelper.GetColumnValue(row, "ContactNumber"),
                Street = FileParsingHelper.GetColumnValue(row, "Street"),
                City = FileParsingHelper.GetColumnValue(row, "City"),
                State = FileParsingHelper.GetColumnValue(row, "State"),
                ZipCode = FileParsingHelper.GetColumnValue(row, "ZipCode"),
                Country = FileParsingHelper.GetColumnValue(row, "Country"),
                CountryCode = FileParsingHelper.GetColumnValue(row, "CountryCode"),
                RiskLevel = FileParsingHelper.GetColumnValue(row, "RiskLevel", required: false)
            };

            clients.Add(client);
        }

        return clients;
    }
}
