using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Application.AuditLogs;
using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Api.Mappers.AuditLogs;

public static class AuditLogContractMapper
{
    public static AuditLogQuery ToQuery(SearchAuditLogsRequest req)
    {
        var page = new PageRequest
        {
            Page = req.Page,
            PageSize = req.PageSize,
            SortBy = req.SortBy,
            SortDir = req.SortDir
        }.Normalize();

        return new AuditLogQuery
        {
            Page = page,
            EntityType = TrimOrNull(req.EntityType),
            EntityId = TrimOrNull(req.EntityId),
            Action = req.Action,
            PerformedByAnalystId = req.PerformedByAnalystId,
            CorrelationId = TrimOrNull(req.CorrelationId),
            FromUtc = req.FromUtc,
            ToUtc = req.ToUtc
        };
    }

    public static AuditLogResponse ToResponse(AuditLogDto dto)
        => new(
            dto.Id,
            dto.EntityType,
            dto.EntityId,
            dto.Action,
            dto.PerformedByAnalystId,
            dto.CorrelationId,
            dto.Before,
            dto.After,
            dto.PerformedAtUtc
        );

    public static PagedResponse<AuditLogResponse> ToPagedResponse(PagedResult<AuditLogDto> paged)
        => new(
            Items: paged.Items.Select(ToResponse).ToList(),
            Page: paged.Page,
            PageSize: paged.PageSize,
            Total: paged.Total,
            TotalPages: paged.TotalPages
        );

    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
