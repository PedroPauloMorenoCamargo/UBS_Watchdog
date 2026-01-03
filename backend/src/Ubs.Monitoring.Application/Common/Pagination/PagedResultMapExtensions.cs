using System;
using System.Linq;

namespace Ubs.Monitoring.Application.Common.Pagination;

/// <summary>
/// Helper extensions to transform paged results while preserving paging metadata.
/// </summary>
public static class PagedResultMapExtensions
{
    public static PagedResult<TOut> Map<TIn, TOut>(
        this PagedResult<TIn> source,
        Func<TIn, TOut> mapper)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (mapper is null)
            throw new ArgumentNullException(nameof(mapper));

        return PagedResult<TOut>.Create(
            source.Items.Select(mapper).ToList(),
            source.Page,
            source.PageSize,
            source.Total
        );
    }
}
