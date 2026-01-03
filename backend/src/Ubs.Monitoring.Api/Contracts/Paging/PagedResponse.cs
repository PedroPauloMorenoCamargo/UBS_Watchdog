namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// API contract for paginated list endpoints.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long Total,
    int TotalPages
);
