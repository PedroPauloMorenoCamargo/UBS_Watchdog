using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Clients;
using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Controller for managing client operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }


    /// <summary>
    /// Creates a new client.
    /// </summary>
    /// <param name="request">
    /// The client creation request containing all required client information.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The created client data with HTTP 201 Created if successful.
    /// Returns HTTP 400 Bad Request if validation fails.
    /// </returns>
    /// <response code="201">Client created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientResponseDto>> CreateClient(
        [FromBody] CreateClientRequest request,
        CancellationToken ct)
    {
        var (result, errorMessage) = await _clientService.CreateClientAsync(request, ct);

        if (result is null)
            return Problem(
                title: "Invalid client data",
                detail: errorMessage ?? "One or more required fields are missing or invalid.",
                statusCode: StatusCodes.Status400BadRequest
            );

        return CreatedAtAction(
            actionName: nameof(GetClientById),
            routeValues: new { id = result.Id },
            value: result
        );
    }

    /// <summary>
    /// Retrieves a paginated list of clients with optional filters and sorting.
    /// </summary>
    /// <param name="query">Query parameters for pagination, sorting, and filtering.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A paginated list of clients matching the filters.
    /// </returns>
    /// <response code="200">Returns the paginated client list.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <remarks>
    /// Query parameters:
    /// - Page.Page: Page number (1-based). Default is 1.
    /// - Page.PageSize: Number of items per page (max 100). Default is 20.
    /// - Page.SortBy: Field to sort by (Name, CountryCode, RiskLevel, KycStatus, CreatedAtUtc, UpdatedAtUtc). Optional. Case-insensitive.
    /// - Page.SortDir: Sort direction (asc or desc). Default is desc.
    /// - CountryCode: ISO country code filter (e.g., "BR", "US"). Optional.
    /// - RiskLevel: Risk level filter (Low, Medium, High). Optional.
    /// - KycStatus: KYC status filter (Pending, Verified, Expired, Rejected). Optional.
    ///
    /// Example: GET /api/clients?Page.Page=1&amp;Page.PageSize=20&amp;Page.SortBy=Name&amp;Page.SortDir=asc&amp;CountryCode=BR
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ClientResponseDto>>> GetClients(
        [FromQuery] ClientQuery query,
        CancellationToken ct = default)
    {
        var result = await _clientService.GetPagedClientsAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific client.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the client.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Detailed client information including accounts, transactions, and cases count.
    /// </returns>
    /// <response code="200">Returns the client details.</response>
    /// <response code="404">Client not found.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ClientDetailDto>> GetClientById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var client = await _clientService.GetClientByIdAsync(id, ct);

        if (client is null)
            return Problem(
                title: "Client not found",
                detail: $"No client found with ID: {id}",
                statusCode: StatusCodes.Status404NotFound
            );

        return Ok(client);
    }

    /// <summary>
    /// Imports multiple clients from a CSV or Excel file.
    /// </summary>
    /// <param name="file">
    /// The CSV or Excel file containing client data.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to cancel the operation.
    /// </param>
    /// <returns>
    /// Import result containing success count and error details.
    /// </returns>
    /// <response code="200">Import completed (may contain partial errors).</response>
    /// <response code="400">Invalid file format or missing file.</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
    /// <response code="413">File size exceeds the 50 MB limit.</response>
    /// <remarks>
    /// This endpoint accepts CSV or Excel files (.csv, .xlsx, .xls) with the following columns:
    ///
    /// - LegalType (required): "Individual" or "Corporate"
    /// - Name (required): Client name
    /// - ContactNumber (required): Phone number
    /// - Street (required): Street address
    /// - City (required): City name
    /// - State (required): State/Province
    /// - ZipCode (required): Postal code
    /// - Country (required): Country name
    /// - CountryCode (required): ISO country code (e.g., "BR", "US")
    /// - RiskLevel (optional): "Low", "Medium", or "High"
    ///
    /// The first row must contain column headers.
    /// Valid clients are saved in batches for optimal performance.
    /// Partial success is possible - successfully imported clients are persisted even if later rows fail.
    /// Invalid rows are reported in the response with line numbers and error messages.
    ///
    /// Maximum file size: 50 MB.
    /// For larger datasets, consider splitting the file into multiple imports.
    /// </remarks>
    [HttpPost("import")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<ImportResultDto>> ImportClients(
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
        var result = await _clientService.ImportClientsFromFileAsync(stream, file.FileName, ct);

        return Ok(result);
    }
}

