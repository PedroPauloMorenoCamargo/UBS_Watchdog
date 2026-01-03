using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Api.Contracts;

namespace Ubs.Monitoring.Api.Extensions;

/// <summary>
/// Mapping helpers from application-layer paged results into API response contracts.
/// </summary>
public static class PagedResponseExtensions
{
    public static PagedResponse<T> ToPagedResponse<T>(this PagedResult<T> result)
        => new(
            Items: result.Items,
            Page: result.Page,
            PageSize: result.PageSize,
            Total: result.Total,
            TotalPages: result.TotalPages
        );
}
