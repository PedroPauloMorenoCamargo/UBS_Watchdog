using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Service for parsing client import files (CSV and Excel).
/// </summary>
public sealed class ClientFileImportService : IClientFileImportService
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
            _ => throw new InvalidOperationException($"Unsupported file format: {extension}. Only CSV and Excel files are supported.")
        };
    }

    /// <summary>
    /// Parses a CSV file.
    /// </summary>
    private List<ClientImportRow> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null, // Ignore missing fields
            HeaderValidated = null,   // Don't validate headers strictly
            TrimOptions = TrimOptions.Trim
        });

        var records = csv.GetRecords<ClientImportRow>().ToList();
        return records;
    }

    /// <summary>
    /// Parses an Excel file (.xlsx or .xls).
    /// </summary>
    private List<ClientImportRow> ParseExcel(Stream stream)
    {
        var clients = new List<ClientImportRow>();

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1); // First sheet

        // Find header row (assumes first row is header)
        var headerRow = worksheet.Row(1);

        // Map column names to indices
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int col = 1; col <= headerRow.CellsUsed().Count(); col++)
        {
            var headerName = headerRow.Cell(col).GetString().Trim();
            columnMap[headerName] = col;
        }

        // Read data rows
        var rows = worksheet.RowsUsed().Skip(1); // Skip header
        foreach (var row in rows)
        {
            var client = new ClientImportRow
            {
                LegalType = GetCellValue(row, columnMap, "LegalType"),
                Name = GetCellValue(row, columnMap, "Name"),
                ContactNumber = GetCellValue(row, columnMap, "ContactNumber"),
                Street = GetCellValue(row, columnMap, "Street"),
                City = GetCellValue(row, columnMap, "City"),
                State = GetCellValue(row, columnMap, "State"),
                ZipCode = GetCellValue(row, columnMap, "ZipCode"),
                Country = GetCellValue(row, columnMap, "Country"),
                CountryCode = GetCellValue(row, columnMap, "CountryCode"),
                RiskLevel = GetCellValue(row, columnMap, "RiskLevel", required: false)
            };

            clients.Add(client);
        }

        return clients;
    }

    /// <summary>
    /// Gets a cell value from an Excel row by column name.
    /// </summary>
    private string GetCellValue(IXLRow row, Dictionary<string, int> columnMap, string columnName, bool required = true)
    {
        if (!columnMap.TryGetValue(columnName, out var colIndex))
        {
            if (required)
                throw new InvalidOperationException($"Required column '{columnName}' not found in Excel file.");
            return string.Empty;
        }

        return row.Cell(colIndex).GetString().Trim();
    }
}
