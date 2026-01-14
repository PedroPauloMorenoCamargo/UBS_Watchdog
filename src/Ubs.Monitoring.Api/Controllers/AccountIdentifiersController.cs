using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.AccountIdentifiers;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing account identifier operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api")]
public sealed class AccountIdentifiersController : ControllerBase
{
    private readonly IAccountIdentifierService _identifierService;

    public AccountIdentifiersController(IAccountIdentifierService identifierService)
    {
        _identifierService = identifierService;
    }

    /// <summary>
    /// Retrieves all identifiers for a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of identifiers belonging to the account.</returns>
    /// <response code="200">Returns the list of identifiers.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("accounts/{accountId:guid}/identifiers")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountIdentifierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AccountIdentifierDto>>> GetIdentifiersByAccountId(
        [FromRoute] Guid accountId,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _identifierService.GetByAccountIdAsync(accountId, ct);

        if (result is null)
        {
            return Problem(
                title: "Account not found",
                detail: errorMessage ?? $"No account found with ID: {accountId}",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new identifier for an account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="request">The identifier creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created identifier data with HTTP 201 Created if successful.</returns>
    /// <response code="201">Identifier created successfully.</response>
    /// <response code="400">Invalid request data or duplicate identifier.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Account not found.</response>
    [HttpPost("accounts/{accountId:guid}/identifiers")]
    [ProducesResponseType(typeof(AccountIdentifierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountIdentifierDto>> CreateIdentifier(
        [FromRoute] Guid accountId,
        [FromBody] CreateAccountIdentifierRequest request,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _identifierService.CreateIdentifierAsync(accountId, request, ct);

        if (result is null)
        {
            if (errorMessage?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Problem(
                    title: "Account not found",
                    detail: errorMessage,
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            return Problem(
                title: "Invalid identifier data",
                detail: errorMessage ?? "One or more required fields are missing or invalid.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return CreatedAtAction(
            actionName: nameof(GetIdentifiersByAccountId),
            routeValues: new { accountId },
            value: result
        );
    }

    /// <summary>
    /// Removes an identifier from an account.
    /// </summary>
    /// <param name="identifierId">The unique identifier of the account identifier to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Identifier removed successfully.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="404">Identifier not found.</response>
    [HttpDelete("account-identifiers/{identifierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveIdentifier(
        [FromRoute] Guid identifierId,
        CancellationToken ct)
    {
        var (success, errorMessage) = await _identifierService.RemoveIdentifierAsync(identifierId, ct);

        if (!success)
        {
            return Problem(
                title: "Identifier not found",
                detail: errorMessage ?? $"No account identifier found with ID: {identifierId}",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return NoContent();
    }
}
