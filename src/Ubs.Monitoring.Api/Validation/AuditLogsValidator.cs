using FluentValidation;
using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Validation.Sorting;
using Ubs.Monitoring.Api.Validation;

namespace Ubs.Monitoring.Api.Validation.AuditLogs;

public sealed class SearchAuditLogsRequestValidator : PageRequestValidatorBase<SearchAuditLogsRequest>
{
    private const int EntityTypeMaxLen = 64;
    private const int EntityIdMaxLen = 80;
    private const int CorrelationIdMaxLen = 100;
    public SearchAuditLogsRequestValidator()
        : base(
            page: x => x.Page,
            pageSize: x => x.PageSize,
            sortBy: x => x.SortBy,
            sortDir: x => x.SortDir,
            isAllowedSortBy: AuditLogSortFields.IsValid)
    {
        RuleFor(x => x)
            .Must(x => x.FromUtc is null || x.ToUtc is null || x.FromUtc <= x.ToUtc)
            .WithMessage("FromUtc must be <= ToUtc.");

        RuleFor(x => x.EntityType)
            .MaximumLength(EntityTypeMaxLen)
            .WithMessage($"EntityType must be at most {EntityTypeMaxLen} characters.")
            .Must(v => v is null || v == v.Trim())
            .WithMessage("EntityType must not contain leading or trailing spaces.");

        RuleFor(x => x.EntityId)
            .MaximumLength(EntityIdMaxLen)
            .WithMessage($"EntityId must be at most {EntityIdMaxLen} characters.")
            .Must(v => v is null || v == v.Trim())
            .WithMessage("EntityId must not contain leading or trailing spaces.");

        RuleFor(x => x.CorrelationId)
            .MaximumLength(CorrelationIdMaxLen)
            .WithMessage($"CorrelationId must be at most {CorrelationIdMaxLen} characters.")
            .Must(v => v is null || v == v.Trim())
            .WithMessage("CorrelationId must not contain leading or trailing spaces.");
    }
}
