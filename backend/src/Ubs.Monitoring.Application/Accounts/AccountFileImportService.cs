using Ubs.Monitoring.Application.Common.FileImport;

namespace Ubs.Monitoring.Application.Accounts;

/// <summary>
/// Account-specific implementation of file parser.
/// Handles CSV and Excel parsing with account-specific field mapping.
/// </summary>
public sealed class AccountFileImportService : IFileParser<AccountImportRow>
{
    /// <summary>
    /// Parses a CSV or Excel file and returns a list of account import rows.
    /// </summary>
    /// <param name="stream">The file stream.</param>
    /// <param name="fileName">The file name (used to determine file type).</param>
    /// <returns>List of parsed account rows.</returns>
    /// <exception cref="InvalidOperationException">Thrown when file format is invalid or unsupported.</exception>
    public List<AccountImportRow> ParseFile(Stream stream, string fileName)
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
    private static List<AccountImportRow> ParseCsv(Stream stream)
    {
        return FileParsingHelper.ParseCsv<AccountImportRow>(stream);
    }

    /// <summary>
    /// Parses an Excel file using generic helper with account-specific mapping.
    /// </summary>
    private static List<AccountImportRow> ParseExcel(Stream stream)
    {
        var rawRows = FileParsingHelper.ParseExcelRaw(stream, worksheetIndex: 1);
        var accounts = new List<AccountImportRow>();

        foreach (var row in rawRows)
        {
            var account = new AccountImportRow
            {
                AccountIdentifier = FileParsingHelper.GetColumnValue(row, "AccountIdentifier"),
                CountryCode = FileParsingHelper.GetColumnValue(row, "CountryCode"),
                AccountType = FileParsingHelper.GetColumnValue(row, "AccountType"),
                CurrencyCode = FileParsingHelper.GetColumnValue(row, "CurrencyCode")
            };

            accounts.Add(account);
        }

        return accounts;
    }
}
