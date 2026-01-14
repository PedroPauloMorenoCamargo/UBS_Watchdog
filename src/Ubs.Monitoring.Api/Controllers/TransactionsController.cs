using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Transactions;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing transaction operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MinPage = 1;

    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    /// <param name="request">The transaction creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created transaction data with HTTP 201 Created if successful.</returns>
    /// <response code="201">Transaction created successfully.</response>
    /// <response code="400">Invalid request data, validation error, or no FX rate available.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Account not found.</response>
    /// <remarks>
    /// Creates a new financial transaction. Supports three types:
    ///
    /// - **Deposit**: Money incoming to the account
    /// - **Withdrawal**: Money outgoing from the account
    /// - **Transfer**: Money moving to another account (requires counterparty info)
    ///
    /// For Transfer transactions, the following fields are required:
    /// - TransferMethod (PIX, TED, or WIRE)
    /// - CpCountryCode (counterparty country)
    /// - CpIdentifierType (CPF, CNPJ, PIX_EMAIL, etc.)
    /// - CpIdentifier (the actual identifier value)
    ///
    /// Currency conversion is automatic. If the transaction currency differs from USD,
    /// the system will look up the latest exchange rate and calculate the base amount.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _transactionService.CreateTransactionAsync(request, ct);

        if (result is null)
        {
            if (errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Problem(
                    title: "Cannot create transaction",
                    detail: errorMessage,
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Problem(
                title: "Invalid transaction data",
                detail: errorMessage ?? "One or more required fields are missing or invalid.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return CreatedAtAction(
            actionName: nameof(GetTransactionById),
            routeValues: new { id = result.Id },
            value: result
        );
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of transactions.
    /// </summary>
    /// <param name="clientId">Optional filter by client ID.</param>
    /// <param name="accountId">Optional filter by account ID.</param>
    /// <param name="type">Optional filter by transaction type (Deposit, Withdrawal, Transfer).</param>
    /// <param name="transferMethod">Optional filter by transfer method (PIX, TED, WIRE).</param>
    /// <param name="dateFrom">Optional filter for transactions on or after this date.</param>
    /// <param name="dateTo">Optional filter for transactions on or before this date.</param>
    /// <param name="minAmount">Optional filter for minimum base amount (in USD).</param>
    /// <param name="maxAmount">Optional filter for maximum base amount (in USD).</param>
    /// <param name="currencyCode">Optional filter by original currency code.</param>
    /// <param name="cpCountryCode">Optional filter by counterparty country code.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100).</param>
    /// <param name="sortBy">Field to sort by (occurredAtUtc, amount, baseAmount, type, createdAtUtc).</param>
    /// <param name="sortDesc">Sort descending (default: true).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of transactions.</returns>
    /// <response code="200">Returns the paginated list of transactions.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedTransactionsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedTransactionsResponseDto>> GetTransactions(
        [FromQuery] Guid? clientId = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] TransactionType? type = null,
        [FromQuery] TransferMethod? transferMethod = null,
        [FromQuery] DateTimeOffset? dateFrom = null,
        [FromQuery] DateTimeOffset? dateTo = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] string? currencyCode = null,
        [FromQuery] string? cpCountryCode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default)
    {
        // Validate pagination
        if (page < MinPage) page = MinPage;
        if (pageSize < 1) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var filter = new TransactionFilterRequest(
            ClientId: clientId,
            AccountId: accountId,
            Type: type,
            TransferMethod: transferMethod,
            DateFrom: dateFrom,
            DateTo: dateTo,
            MinAmount: minAmount,
            MaxAmount: maxAmount,
            CurrencyCode: currencyCode,
            CpCountryCode: cpCountryCode,
            Page: page,
            PageSize: pageSize,
            SortBy: sortBy,
            SortDescending: sortDesc
        );

        var result = await _transactionService.GetTransactionsAsync(filter, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific transaction.
    /// </summary>
    /// <param name="id">The unique identifier of the transaction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed transaction information including account and client data.</returns>
    /// <response code="200">Returns the transaction details.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Transaction not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDetailDto>> GetTransactionById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id, ct);

        if (transaction is null)
        {
            return Problem(
                title: "Transaction not found",
                detail: $"No transaction found with ID: {id}",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(transaction);
    }

    /// <summary>
    /// Imports multiple transactions from a CSV or Excel file.
    /// </summary>
    /// <param name="file">The CSV or Excel file containing transaction data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Import result containing success count and error details.</returns>
    /// <response code="200">Import completed (may contain partial errors).</response>
    /// <response code="400">Invalid file format or missing file.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="413">File size exceeds the 50 MB limit.</response>
    /// <remarks>
    /// This endpoint accepts CSV or Excel files (.csv, .xlsx, .xls) with the following columns:
    ///
    /// **Required columns:**
    /// - AccountIdentifier: The account identifier for the transaction
    /// - Type: Transaction type (Deposit, Withdrawal, Transfer)
    /// - Amount: Transaction amount (positive decimal)
    /// - CurrencyCode: ISO currency code (e.g., BRL, USD, EUR)
    /// - OccurredAtUtc: Date/time of the transaction (ISO 8601 format)
    ///
    /// **Required for Transfer transactions:**
    /// - TransferMethod: PIX, TED, or WIRE
    /// - CpIdentifierType: CPF, CNPJ, PIX_EMAIL, PIX_PHONE, PIX_RANDOM, IBAN, etc.
    /// - CpIdentifier: The actual identifier value
    /// - CpCountryCode: ISO country code (e.g., BR, US)
    ///
    /// **Optional columns:**
    /// - CpName: Counterparty name
    /// - CpBank: Counterparty bank name
    /// - CpBranch: Counterparty branch
    /// - CpAccount: Counterparty account number
    ///
    /// The first row must contain column headers.
    /// Currency conversion to USD is automatic using the latest available FX rate.
    /// Partial success is possible - valid transactions are persisted even if later rows fail.
    ///
    /// Maximum file size: 50 MB.
    /// </remarks>
    [HttpPost("import")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(TransactionImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<TransactionImportResultDto>> ImportTransactions(
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Problem(
                title: "No file provided",
                detail: "Please upload a CSV or Excel file.",
                statusCode: StatusCodes.Status400BadRequest
            );

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".xlsx" && extension != ".xls")
            return Problem(
                title: "Invalid file format",
                detail: "Only CSV (.csv) and Excel (.xlsx, .xls) files are supported.",
                statusCode: StatusCodes.Status400BadRequest
            );

        using var stream = file.OpenReadStream();
        var (result, errorMessage) = await _transactionService.ImportTransactionsFromFileAsync(stream, file.FileName, ct);

        if (result is null)
        {
            return Problem(
                title: "Import failed",
                detail: errorMessage ?? "Failed to import transactions from file.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(result);
    }
}
