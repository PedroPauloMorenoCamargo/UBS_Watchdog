using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Application.Common.Pagination;
using Ubs.Monitoring.Application.ComplianceRules;

namespace Ubs.Monitoring.Api.Mappers;

public static class ComplianceRuleContractMapper
{
    public static ComplianceRuleQuery ToQuery(SearchComplianceRulesRequest req)
    {
        var page = new PageRequest
        {
            Page = req.Page,
            PageSize = req.PageSize,
            SortBy = req.SortBy,
            SortDir = req.SortDir
        }.Normalize();

        return new ComplianceRuleQuery
        {
            Page = page,
            RuleType = req.RuleType,
            IsActive = req.IsActive,
            Severity = req.Severity,
            Scope = string.IsNullOrWhiteSpace(req.Scope) ? null : req.Scope.Trim()
        };
    }

    public static PatchComplianceRuleDto ToPatchDto(PatchComplianceRuleRequest req)
        => new(
            Name: req.Name,
            IsActive: req.IsActive,
            Severity: req.Severity,
            Scope: req.Scope,
            Parameters: req.Parameters
        );

    public static ComplianceRuleResponse ToResponse(ComplianceRuleDto dto)
        => new(
            Id: dto.Id,
            Code: dto.Code,
            RuleType: dto.RuleType,
            Name: dto.Name,
            IsActive: dto.IsActive,
            Severity: dto.Severity,
            Scope: dto.Scope,
            Parameters: dto.Parameters,
            CreatedAtUtc: dto.CreatedAtUtc,
            UpdatedAtUtc: dto.UpdatedAtUtc
        );

    public static PagedResponse<ComplianceRuleResponse> ToPagedResponse(PagedResult<ComplianceRuleDto> paged)
        => new(
            Items: paged.Items.Select(ToResponse).ToList(),
            Page: paged.Page,
            PageSize: paged.PageSize,
            Total: paged.Total,
            TotalPages: paged.TotalPages
        );
}
