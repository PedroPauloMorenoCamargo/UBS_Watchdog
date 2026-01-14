using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Ubs.Monitoring.Application.Common.FileExport;

/// <summary>
/// Helper class for generating PDF exports using QuestPDF.
/// Provides generic PDF report generation with tables, headers, and footers.
/// </summary>
public static class PdfExportHelper
{
    /// <summary>
    /// Static constructor to configure QuestPDF license.
    /// QuestPDF is free for companies with less than $1M annual revenue.
    /// </summary>
    static PdfExportHelper()
    {
        // Configure QuestPDF license type
        // Community license is free for companies with less than $1M annual gross revenue
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Exports tabular data to PDF format.
    /// </summary>
    /// <param name="headers">Column headers.</param>
    /// <param name="rows">Data rows (each row is a list of cell values).</param>
    /// <param name="title">Report title (optional).</param>
    /// <param name="options">Export options for customization.</param>
    /// <returns>PDF file content as byte array.</returns>
    /// <exception cref="ArgumentException">Thrown when headers is null/empty or rows have inconsistent column counts.</exception>
    public static byte[] ExportToPdf(
        List<string> headers,
        List<List<string>> rows,
        string? title = null,
        ExportOptions? options = null)
    {
        // Validate inputs to prevent malformed PDF
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);

        if (headers.Count == 0)
            throw new ArgumentException("Headers list cannot be empty. At least one column header is required.", nameof(headers));

        // Validate row column counts match header count
        var expectedColumns = headers.Count;
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count != expectedColumns)
            {
                throw new ArgumentException(
                    $"Row {i + 1} has {row.Count} columns but expected {expectedColumns} (based on headers count).",
                    nameof(rows));
            }
        }

        options ??= new ExportOptions();
        var reportTitle = title ?? options.Title ?? "Report";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(header => ComposeHeader(header, reportTitle, options));
                
                page.Content().Element(content => ComposeContent(content, headers, rows));
                
                page.Footer().Element(ComposeFooter);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Exports tabular data to PDF with strongly-typed records.
    /// </summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="records">Collection of records to export.</param>
    /// <param name="columnMappings">Dictionary mapping property names to display headers.</param>
    /// <param name="title">Report title.</param>
    /// <param name="options">Export options.</param>
    /// <returns>PDF file content as byte array.</returns>
    /// <exception cref="ArgumentException">Thrown when columnMappings is null/empty or no matching properties found.</exception>
    public static byte[] ExportToPdf<T>(
        IEnumerable<T> records,
        Dictionary<string, string> columnMappings,
        string? title = null,
        ExportOptions? options = null) where T : class
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(columnMappings);

        if (columnMappings.Count == 0)
            throw new ArgumentException("Column mappings cannot be empty. At least one property mapping is required.", nameof(columnMappings));

        var properties = typeof(T).GetProperties()
            .Where(p => columnMappings.ContainsKey(p.Name))
            .ToList();

        if (properties.Count == 0)
            throw new ArgumentException(
                $"No properties of type '{typeof(T).Name}' match the provided column mappings. " +
                $"Available properties: {string.Join(", ", typeof(T).GetProperties().Select(p => p.Name))}",
                nameof(columnMappings));

        var headers = properties
            .Select(p => columnMappings.GetValueOrDefault(p.Name, p.Name))
            .ToList();

        var rows = records.Select(record =>
            properties.Select(p => FormatValue(p.GetValue(record), options))
            .ToList()
        ).ToList();

        return ExportToPdf(headers, rows, title, options);
    }

    /// <summary>
    /// Composes the header section of the PDF.
    /// </summary>
    private static void ComposeHeader(IContainer container, string title, ExportOptions options)
    {
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Medium)
            .PaddingBottom(10)
            .Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item()
                        .Text(title)
                        .Bold()
                        .FontSize(18)
                        .FontColor(Colors.Blue.Darken2);

                    if (options.IncludeTimestamp)
                    {
                        column.Item()
                            .Text($"Generated: {DateTime.UtcNow.ToString(options.DateFormat, CultureInfo.InvariantCulture)} UTC")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                row.ConstantItem(100)
                    .AlignRight()
                    .AlignMiddle()
                    .Text("UBS Monitoring")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });
    }

    /// <summary>
    /// Composes the main content (data table) of the PDF.
    /// </summary>
    private static void ComposeContent(IContainer container, List<string> headers, List<List<string>> rows)
    {
        container
            .PaddingVertical(15)
            .Table(table =>
            {
                // Define columns with equal width
                table.ColumnsDefinition(columns =>
                {
                    foreach (var _ in headers)
                    {
                        columns.RelativeColumn();
                    }
                });

                // Header row
                table.Header(header =>
                {
                    foreach (var headerText in headers)
                    {
                        header.Cell()
                            .Background(Colors.Blue.Darken2)
                            .Padding(5)
                            .Text(headerText)
                            .Bold()
                            .FontSize(9)
                            .FontColor(Colors.White);
                    }
                });

                // Data rows with alternating colors
                var rowIndex = 0;
                foreach (var row in rows)
                {
                    var backgroundColor = rowIndex % 2 == 0
                        ? Colors.White
                        : Colors.Grey.Lighten4;

                    foreach (var cell in row)
                    {
                        table.Cell()
                            .Background(backgroundColor)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(5)
                            .Text(cell ?? string.Empty)
                            .FontSize(8);
                    }

                    rowIndex++;
                }
            });
    }

    /// <summary>
    /// Composes the footer section with page numbers.
    /// </summary>
    private static void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor(Colors.Grey.Lighten1)
            .PaddingTop(5)
            .Row(row =>
            {
                row.RelativeItem()
                    .AlignLeft()
                    .Text("UBS Monitoring System - Confidential")
                    .FontSize(7)
                    .FontColor(Colors.Grey.Darken1);

                row.RelativeItem()
                    .AlignRight()
                    .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1))
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
    }

    /// <summary>
    /// Formats a value for display in the PDF based on its type.
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
