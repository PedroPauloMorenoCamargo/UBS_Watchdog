using Ubs.Monitoring.Application.Common.FileImport;

namespace Ubs.Monitoring.Application.Transactions;

/// <summary>
/// Transaction-specific implementation of file parser.
/// Handles CSV and Excel parsing with transaction-specific field mapping.
/// </summary>
public sealed class TransactionFileImportService : IFileParser<TransactionImportRow>
{
    /// <summary>
    /// Parses a CSV or Excel file and returns a list of transaction import rows.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The file name (used to determine file type).</param>
    /// <returns>List of parsed transaction rows.</returns>
    /// <exception cref="InvalidOperationException">Thrown when file format is invalid or unsupported.</exception>
    public List<TransactionImportRow> ParseFile(Stream stream, string fileName)
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
    private static List<TransactionImportRow> ParseCsv(Stream stream)
    {
        return FileParsingHelper.ParseCsv<TransactionImportRow>(stream);
    }

    /// <summary>
    /// Parses an Excel file using generic helper with transaction-specific mapping.
    /// </summary>
    private static List<TransactionImportRow> ParseExcel(Stream stream)
    {
        var rawRows = FileParsingHelper.ParseExcelRaw(stream, worksheetIndex: 1);
        var transactions = new List<TransactionImportRow>();

        foreach (var row in rawRows)
        {
            var transaction = new TransactionImportRow
            {
                // Required fields
                AccountIdentifier = FileParsingHelper.GetColumnValue(row, "AccountIdentifier"),
                Type = FileParsingHelper.GetColumnValue(row, "Type"),
                Amount = FileParsingHelper.GetColumnValue(row, "Amount"),
                CurrencyCode = FileParsingHelper.GetColumnValue(row, "CurrencyCode"),
                OccurredAtUtc = FileParsingHelper.GetColumnValue(row, "OccurredAtUtc"),

                // Transfer-specific fields (optional for non-transfers)
                TransferMethod = FileParsingHelper.GetColumnValue(row, "TransferMethod", required: false),
                CpIdentifierType = FileParsingHelper.GetColumnValue(row, "CpIdentifierType", required: false),
                CpIdentifier = FileParsingHelper.GetColumnValue(row, "CpIdentifier", required: false),
                CpCountryCode = FileParsingHelper.GetColumnValue(row, "CpCountryCode", required: false),

                // Optional counterparty fields
                CpName = FileParsingHelper.GetColumnValue(row, "CpName", required: false),
                CpBank = FileParsingHelper.GetColumnValue(row, "CpBank", required: false),
                CpBranch = FileParsingHelper.GetColumnValue(row, "CpBranch", required: false),
                CpAccount = FileParsingHelper.GetColumnValue(row, "CpAccount", required: false)
            };

            transactions.Add(transaction);
        }

        return transactions;
    }
}
