using ClosedXML.Excel;

namespace Ubs.Monitoring.Application.Common.FileExport;

/// <summary>
/// Helper class for generating Excel exports using ClosedXML.
/// Provides generic Excel generation functionality with formatting and styling.
/// </summary>
public static class ExcelExportHelper
{
    /// <summary>
    /// Exports a collection of objects to Excel format (.xlsx).
    /// </summary>
    /// <typeparam name="T">Type of objects to export.</typeparam>
    /// <param name="records">Collection of records to export.</param>
    /// <param name="sheetName">Name of the worksheet (default: "Data").</param>
    /// <param name="options">Export options for customization.</param>
    /// <returns>Excel file content as byte array.</returns>
    public static byte[] ExportToExcel<T>(
        IEnumerable<T> records,
        string sheetName = "Data",
        ExportOptions? options = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(records);

        var recordList = records.ToList();
        var properties = typeof(T).GetProperties();

        if (properties.Length == 0)
            throw new ArgumentException($"Type '{typeof(T).Name}' has no public properties to export.");

        var headers = properties.Select(p =>
            options?.CustomHeaders?.GetValueOrDefault(p.Name, p.Name) ?? p.Name
        ).ToList();

        var rows = recordList.Select(record =>
            properties.Select(p => FormatValue(p.GetValue(record), options)).ToList()
        ).ToList();

        return ExportToExcelRaw(headers, rows, sheetName, options);
    }

    /// <summary>
    /// Exports data to Excel with custom headers and raw data.
    /// </summary>
    /// <param name="headers">Column headers.</param>
    /// <param name="rows">Data rows (each row is a list of cell values).</param>
    /// <param name="sheetName">Name of the worksheet (default: "Data").</param>
    /// <param name="options">Export options for customization.</param>
    /// <returns>Excel file content as byte array.</returns>
    public static byte[] ExportToExcelRaw(
        List<string> headers,
        List<List<string>> rows,
        string sheetName = "Data",
        ExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        if (headers.Count == 0)
            throw new ArgumentException("Headers list cannot be empty.", nameof(headers));

        options ??= new ExportOptions();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Add title if provided
        var startRow = 1;
        if (!string.IsNullOrWhiteSpace(options.Title))
        {
            worksheet.Cell(1, 1).Value = options.Title;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.DarkBlue;
            worksheet.Range(1, 1, 1, headers.Count).Merge();
            startRow = 2;

            if (options.IncludeTimestamp)
            {
                worksheet.Cell(2, 1).Value = $"Generated: {DateTime.UtcNow.ToString(options.DateFormat)} UTC";
                worksheet.Cell(2, 1).Style.Font.FontSize = 10;
                worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
                worksheet.Range(2, 1, 2, headers.Count).Merge();
                startRow = 4; // Leave a blank row
            }
        }

        // Write headers
        for (var col = 0; col < headers.Count; col++)
        {
            var cell = worksheet.Cell(startRow, col + 1);
            cell.Value = headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        // Write data rows
        for (var rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var row = rows[rowIdx];
            var excelRow = startRow + 1 + rowIdx;

            // Alternating row colors
            var backgroundColor = rowIdx % 2 == 0 ? XLColor.White : XLColor.FromHtml("#F5F5F5");

            for (var col = 0; col < row.Count && col < headers.Count; col++)
            {
                var cell = worksheet.Cell(excelRow, col + 1);
                cell.Value = row[col] ?? string.Empty;
                cell.Style.Fill.BackgroundColor = backgroundColor;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
                cell.Style.Border.BottomBorderColor = XLColor.LightGray;
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

  
        if (rows.Count > 0)
        {
            var tableRange = worksheet.Range(
                startRow, 1,
                startRow + rows.Count, headers.Count
            );
            tableRange.SetAutoFilter();
        }

        // Freeze header row
        worksheet.SheetView.FreezeRows(startRow);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Formats a value for display in Excel based on its type.
    /// </summary>
    private static string FormatValue(object? value, ExportOptions? options)
    {
        if (value == null)
            return string.Empty;

        options ??= new ExportOptions();

        return value switch
        {
            DateTime dt => dt.ToString(options.DateFormat),
            DateTimeOffset dto => dto.ToString(options.DateFormat),
            decimal d => d.ToString(options.CurrencyFormat),
            double dbl => dbl.ToString(options.CurrencyFormat),
            float f => f.ToString(options.CurrencyFormat),
            bool b => b ? "Yes" : "No",
            Enum e => e.ToString(),
            _ => value.ToString() ?? string.Empty
        };
    }
}
