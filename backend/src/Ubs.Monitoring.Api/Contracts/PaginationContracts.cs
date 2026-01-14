namespace Ubs.Monitoring.Api.Contracts;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long Total,
    int TotalPages
);
