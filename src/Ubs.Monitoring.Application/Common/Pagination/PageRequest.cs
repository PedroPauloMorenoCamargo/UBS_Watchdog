namespace Ubs.Monitoring.Application.Common.Pagination;

public sealed record PageRequest
{
    public int Page { get; init; } = PaginationDefaults.DefaultPage;
    public int PageSize { get; init; } = PaginationDefaults.DefaultPageSize;
    public string? SortBy { get; init; }
    public string? SortDir { get; init; } = "desc";

    /// <summary>
    /// Normalizes Page/PageSize/SortDir into safe, consistent values.
    /// </summary>
    public PageRequest Normalize(
        int defaultPageSize = PaginationDefaults.DefaultPageSize,
        int maxPageSize = PaginationDefaults.MaxPageSize)
    {
        var page = Page < 1 ? PaginationDefaults.DefaultPage : Page;

        var size = PageSize < 1 ? defaultPageSize : PageSize;
        if (size > maxPageSize) size = maxPageSize;

        var dir = (SortDir ?? "desc").Trim().ToLowerInvariant();
        if (dir is not ("asc" or "desc")) dir = "desc";

        var sortBy = string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim();

        return this with
        {
            Page = page,
            PageSize = size,
            SortDir = dir,
            SortBy = sortBy
        };
    }
}
