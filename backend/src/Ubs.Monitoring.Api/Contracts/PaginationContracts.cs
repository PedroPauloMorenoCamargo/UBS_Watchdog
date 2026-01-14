    namespace Ubs.Monitoring.Api.Contracts;

/// <summary>
/// Represents a paginated response returned by the API.
/// </summary>
/// <typeparam name="T">
/// The type of the items contained in the paged result.
/// </typeparam>
/// <param name="Items">
/// The collection of items for the current page.
/// </param>
/// <param name="Page">
/// The 1-based index of the current page.
/// </param>
/// <param name="PageSize">
/// The maximum number of items per page.
/// </param>
/// <param name="Total">
/// The total number of items available across all pages.
/// </param>
/// <param name="TotalPages">
/// The total number of pages available based on <paramref name="Total"/>
/// and <paramref name="PageSize"/>.
/// </param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long Total,
    int TotalPages
);
