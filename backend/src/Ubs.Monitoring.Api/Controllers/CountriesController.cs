using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ubs.Monitoring.Application.Countries;

namespace Ubs.Monitoring.Api.Controllers;

/// <summary>
/// Manages country data for client registration and compliance tracking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ICountryService countryService, ILogger<CountriesController> logger)
    {
        _countryService = countryService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all available countries for dropdown lists in client registration.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of countries with ISO alpha-2 codes, names, and risk status.</returns>
    /// <response code="200">Returns the list of all countries.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CountryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CountryResponseDto>>> GetCountries(
        CancellationToken ct = default)
    {
        _logger.LogInformation("GET /api/countries - Retrieving all countries");

        var countries = await _countryService.GetAllCountriesAsync(ct);

        _logger.LogInformation("Returning {Count} countries", countries.Count);

        return Ok(countries);
    }

    /// <summary>
    /// Updates the risk level of a specific country for compliance purposes.
    /// </summary>
    /// <param name="code">ISO alpha-2 country code (e.g., BR, US, GB).</param>
    /// <param name="request">Request containing the new risk level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated country information.</returns>
    /// <response code="200">Country risk level updated successfully.</response>
    /// <response code="400">Invalid risk level value.</response>
    /// <response code="401">Unauthorized - authentication required.</response>
    /// <response code="404">Country not found.</response>
    [HttpPatch("{code}/risk-level")]
    [ProducesResponseType(typeof(CountryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CountryResponseDto>> UpdateRiskLevel(
        string code,
        [FromBody] UpdateCountryRiskLevelRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("PATCH /api/countries/{Code}/risk-level - Updating risk level to {RiskLevel}", code, request.NewRiskLevel);

        var result = await _countryService.UpdateRiskLevelAsync(code, request, ct);

        if (result == null)
        {
            _logger.LogWarning("Country {Code} not found", code);
            return NotFound(new { message = $"Country with code '{code}' not found." });
        }

        _logger.LogInformation("Country {Code} risk level updated to {RiskLevel}", code, result.RiskLevel);

        return Ok(result);
    }
}
