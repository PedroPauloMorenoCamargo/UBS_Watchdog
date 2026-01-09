using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ubs.Monitoring.Application.Common.FileImport;

/// <summary>
/// Generic helper for parsing CSV and Excel files.
/// Handles file format detection and raw data extraction.
/// </summary>
public static class FileParsingHelper
{
    /// <summary>
    /// Supported file extensions.
    /// </summary>
    public static readonly string[] SupportedExtensions = { ".csv", ".xlsx", ".xls" };

    /// <summary>
    /// Validates if the file extension is supported.
    /// </summary>
    /// <param name="fileName">File name to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when file format is unsupported.</exception>
    public static void ValidateFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!SupportedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Unsupported file format: {extension}. Supported formats: {string.Join(", ", SupportedExtensions)}");
        }
    }

    /// <summary>
    /// Determines if the file is a CSV file based on extension.
    /// </summary>
    public static bool IsCsvFile(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() == ".csv";
    }

    /// <summary>
    /// Determines if the file is an Excel file based on extension.
    /// </summary>
    public static bool IsExcelFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".xlsx" or ".xls";
    }

    /// <summary>
    /// Parses a CSV file and returns strongly-typed records.
    /// </summary>
    /// <typeparam name="T">Type of record to parse into.</typeparam>
    /// <param name="stream">File stream.</param>
    /// <param name="csvConfig">Optional CSV configuration.</param>
    /// <returns>List of parsed records.</returns>
    public static List<T> ParseCsv<T>(Stream stream, CsvConfiguration? csvConfig = null)
    {
        csvConfig ??= new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(reader, csvConfig);

        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Parses an Excel file and extracts raw data as a list of dictionaries (column name → value).
    /// </summary>
    /// <param name="stream">File stream.</param>
    /// <param name="worksheetIndex">Worksheet index (1-based, default = 1).</param>
    /// <returns>List of rows, where each row is a dictionary of column name to cell value.</returns>
    public static List<Dictionary<string, string>> ParseExcelRaw(Stream stream, int worksheetIndex = 1)
    {
        var rows = new List<Dictionary<string, string>>();

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(worksheetIndex);

        // Extract header row
        var headerRow = worksheet.Row(1);
        var columnMap = new Dictionary<int, string>(); 

        foreach (var cell in headerRow.CellsUsed())
        {
            var colIndex = cell.Address.ColumnNumber;
            var headerName = cell.GetString().Trim();
            columnMap[colIndex] = headerName;
        }

        // Extract data rows
        var dataRows = worksheet.RowsUsed().Skip(1); 
        foreach (var row in dataRows)
        {
            var rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in row.CellsUsed())
            {
                var colIndex = cell.Address.ColumnNumber;
                if (columnMap.TryGetValue(colIndex, out var columnName))
                {
                    rowData[columnName] = cell.GetString().Trim();
                }
            }
            rows.Add(rowData);
        }

        return rows;
    }

    /// <summary>
    /// Gets a value from a dictionary with case-insensitive key lookup.
    /// </summary>
    /// <param name="row">Dictionary containing row data.</param>
    /// <param name="columnName">Column name to lookup.</param>
    /// <param name="required">Whether the column is required.</param>
    /// <returns>Column value or empty string if not required and not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required column is missing.</exception>
    public static string GetColumnValue(Dictionary<string, string> row, string columnName, bool required = true)
    {
        if (row.TryGetValue(columnName, out var value))
        {
            return value;
        }

        if (required)
        {
            throw new InvalidOperationException($"Required column '{columnName}' not found.");
        }

        return string.Empty;
    }
}
