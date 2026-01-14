using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Accounts;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing account operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api")]
public sealed class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Creates a new account for a client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="request">The account creation request containing all required account information.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created account data with HTTP 201 Created if successful.</returns>
    /// <response code="201">Account created successfully.</response>
    /// <response code="400">Invalid request data or account identifier already exists.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Client not found.</response>
    [HttpPost("clients/{clientId:guid}/accounts")]
    [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponseDto>> CreateAccount(
        [FromRoute] Guid clientId,
        [FromBody] CreateAccountRequest request,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _accountService.CreateAccountAsync(clientId, request, ct);

        if (result is null)
        {
            if (errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Problem(
                    title: "Cannot create account",
                    detail: $"Cannot create account for non-existent client. {errorMessage}",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Problem(
                title: "Invalid account data",
                detail: errorMessage ?? "One or more required fields are missing or invalid.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return CreatedAtAction(
            actionName: nameof(GetAccountById),
            routeValues: new { accountId = result.Id },
            value: result
        );
    }

    /// <summary>
    /// Retrieves all accounts for a specific client.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of accounts belonging to the client.</returns>
    /// <response code="200">Returns the list of accounts.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Client not found.</response>
    [HttpGet("clients/{clientId:guid}/accounts")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AccountResponseDto>>> GetAccountsByClientId(
        [FromRoute] Guid clientId,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _accountService.GetAccountsByClientIdAsync(clientId, ct);

        if (result is null)
        {
            return Problem(
                title: "Client not found",
                detail: $"Cannot retrieve accounts for client '{clientId}': client does not exist.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Detailed account information including identifiers.</returns>
    /// <response code="200">Returns the account details.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("accounts/{accountId:guid}")]
    [ProducesResponseType(typeof(AccountDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDetailDto>> GetAccountById(
        [FromRoute] Guid accountId,
        CancellationToken ct)
    {
        var account = await _accountService.GetAccountByIdAsync(accountId, ct);

        if (account is null)
        {
            return Problem(
                title: "Account not found",
                detail: $"No account found with ID: {accountId}",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(account);
    }

    /// <summary>
    /// Imports multiple accounts for a client from a CSV or Excel file.
    /// </summary>
    /// <param name="clientId">The unique identifier of the client.</param>
    /// <param name="file">The CSV or Excel file containing account data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Import result containing success count and error details.</returns>
    /// <response code="200">Import completed (may contain partial errors).</response>
    /// <response code="400">Invalid file format or missing file.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="413">File size exceeds the 50 MB limit.</response>
    /// <remarks>
    /// This endpoint accepts CSV or Excel files (.csv, .xlsx, .xls) with the following columns:
    ///
    /// - AccountIdentifier (required): Unique account identifier
    /// - CountryCode (required): ISO country code (e.g., "BR", "US")
    /// - AccountType (required): "Checking", "Savings", "Investment", or "Other"
    /// - CurrencyCode (required): ISO currency code (e.g., "BRL", "USD", "EUR")
    ///
    /// The first row must contain column headers.
    /// Valid accounts are saved in batches for optimal performance.
    /// Partial success is possible - successfully imported accounts are persisted even if later rows fail.
    /// Invalid rows are reported in the response with line numbers and error messages.
    ///
    /// Maximum file size: 50 MB.
    /// </remarks>
    [HttpPost("clients/{clientId:guid}/accounts/import")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(AccountImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<AccountImportResultDto>> ImportAccounts(
        [FromRoute] Guid clientId,
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
        var (result, errorMessage) = await _accountService.ImportAccountsFromFileAsync(clientId, stream, file.FileName, ct);

        if (result is null)
        {
            return Problem(
                title: "Cannot import accounts",
                detail: $"Cannot import accounts for non-existent client. Client ID '{clientId}' not found.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(result);
    }

}
