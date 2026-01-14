namespace Ubs.Monitoring.Application.Common.Pagination;

/// <summary>
/// Standard paged result container returned by repositories/services.
/// </summary>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required long Total { get; init; }

    /// <summary>Total number of pages computed from Total and PageSize.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, long total)
        => new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
}
