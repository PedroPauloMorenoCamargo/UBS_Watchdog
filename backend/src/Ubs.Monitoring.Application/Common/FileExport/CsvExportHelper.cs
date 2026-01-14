using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ubs.Monitoring.Application.Common.FileExport;

/// <summary>
/// Helper class for generating CSV exports.
/// Provides generic CSV generation functionality that can be used by specific exporters.
/// </summary>
public static class CsvExportHelper
{
    /// <summary>
    /// Exports a collection of objects to CSV format.
    /// </summary>
    /// <typeparam name="T">Type of objects to export.</typeparam>
    /// <param name="records">Collection of records to export.</param>
    /// <param name="csvConfig">Optional CSV configuration.</param>
    /// <returns>CSV content as byte array (UTF-8 encoded).</returns>
    public static byte[] ExportToCsv<T>(IEnumerable<T> records, CsvConfiguration? csvConfig = null)
    {
        csvConfig ??= new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        };

        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, csvConfig);

        csv.WriteRecords(records);
        writer.Flush();

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Exports data to CSV with custom headers.
    /// </summary>
    /// <param name="headers">Column headers.</param>
    /// <param name="rows">Data rows (each row is a list of cell values).</param>
    /// <returns>CSV content as byte array (UTF-8 encoded).</returns>
    public static byte[] ExportToCsvRaw(List<string> headers, List<List<string>> rows)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

        // Write headers
        writer.WriteLine(string.Join(",", headers.Select(EscapeCsvValue)));

        // Write rows
        foreach (var row in rows)
        {
            writer.WriteLine(string.Join(",", row.Select(EscapeCsvValue)));
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Escapes CSV values (handles commas, quotes, newlines).
    /// </summary>
    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // If value contains comma, quote, or newline, wrap in quotes and escape quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
