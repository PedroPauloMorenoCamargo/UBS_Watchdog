using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Api.Validation.Sorting;
using FluentValidation;
using System.Text.Json;

namespace Ubs.Monitoring.Api.Validation;

public sealed class SearchComplianceRulesRequestValidator : PageRequestValidatorBase<SearchComplianceRulesRequest>
{
    public SearchComplianceRulesRequestValidator()
        : base(
            page: x => x.Page,
            pageSize: x => x.PageSize,
            sortBy: x => x.SortBy,
            sortDir: x => x.SortDir,
            isAllowedSortBy: ComplianceRuleSortFields.IsValid)
    {
        RuleFor(x => x.Scope)
            .MaximumLength(64).WithMessage("Scope must be at most 64 characters.")
            .Must(v => v is null || v == v.Trim()).WithMessage("Scope must not contain leading or trailing spaces.");
    }
}


public sealed class PatchComplianceRuleRequestValidator
    : AbstractValidator<PatchComplianceRuleRequest>
{
    public PatchComplianceRuleRequestValidator()
    {
        RuleFor(x => x).Must(HasAtLeastOneField).WithMessage("At least one field must be provided.");

        RuleFor(x => x.Name)
            .MaximumLength(200).Must(v => v is null || v == v.Trim()).WithMessage("Name must not contain leading or trailing spaces.");

        RuleFor(x => x.Scope)
            .MaximumLength(64).Must(v => v is null || v == v.Trim()).WithMessage("Scope must not contain leading or trailing spaces.");

        RuleFor(x => x.Parameters)
            .Must(p => p is null || p.Value.ValueKind == JsonValueKind.Object).WithMessage("Parameters must be a JSON object.");
    }

    private static bool HasAtLeastOneField(PatchComplianceRuleRequest req)
        => req.Name is not null
        || req.IsActive is not null
        || req.Severity is not null
        || req.Scope is not null
        || req.Parameters is not null;
}