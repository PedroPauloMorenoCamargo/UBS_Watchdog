using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Application.Clients;

/// <summary>
/// Query object for filtering and paginating clients.
/// Combines pagination (via PageRequest) with Client-specific filters.
/// </summary>
public sealed record ClientQuery
{
    /// <summary>
    /// Pagination and sorting parameters.
    /// </summary>
    public PageRequest Page { get; init; } = new();

    /// <summary>
    /// Filter by country code (ISO 3166-1 alpha-2).
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    /// Filter by risk level.
    /// </summary>
    public RiskLevel? RiskLevel { get; init; }

    /// <summary>
    /// Filter by KYC status.
    /// </summary>
    public KycStatus? KycStatus { get; init; }
}
