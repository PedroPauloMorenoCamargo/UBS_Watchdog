using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Infrastructure.Persistence.Pagination;

/// <summary>
/// EF Core-friendly paging extensions for IQueryable.
/// </summary>
public static class QueryablePagingExtensions
{
    /// <summary>
    /// Executes server-side paging and returns a PagedResult projected to TOut.
    /// </summary>
    public static async Task<PagedResult<TOut>> ToPagedResultAsync<TIn, TOut>(
        this IQueryable<TIn> query,
        PageRequest pageRequest,
        Expression<Func<TIn, TOut>> selector,
        CancellationToken ct)
    {
        var pr = pageRequest.Normalize();

        var total = await query.LongCountAsync(ct);

        var items = await query
            .Skip((pr.Page - 1) * pr.PageSize)
            .Take(pr.PageSize)
            .Select(selector)
            .ToListAsync(ct);

        return PagedResult<TOut>.Create(items, pr.Page, pr.PageSize, total);
    }

    /// <summary>
    /// Executes server-side paging and returns a PagedResult of the same type.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest pageRequest,
        CancellationToken ct)
    {
        var pr = pageRequest.Normalize();

        var total = await query.LongCountAsync(ct);

        var items = await query
            .Skip((pr.Page - 1) * pr.PageSize)
            .Take(pr.PageSize)
            .ToListAsync(ct);

        return PagedResult<T>.Create(items, pr.Page, pr.PageSize, total);
    }
}
