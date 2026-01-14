using Ubs.Monitoring.Api.Contracts;
using Ubs.Monitoring.Api.Validation;
using Ubs.Monitoring.Api.Validation.Sorting;
using FluentValidation;
using System.Text.Json;

namespace Ubs.Monitoring.Api.Validation;

public sealed class SearchComplianceRulesRequestValidator : PageRequestValidatorBase<SearchComplianceRulesRequest>
{
    private const int ScopeMaxLen = 20;
    public SearchComplianceRulesRequestValidator()
        : base(
            page: x => x.Page,
            pageSize: x => x.PageSize,
            sortBy: x => x.SortBy,
            sortDir: x => x.SortDir,
            isAllowedSortBy: ComplianceRuleSortFields.IsValid)
    {
        RuleFor(x => x.Scope)
            .MaximumLength(ScopeMaxLen).WithMessage("Scope must be at most 64 characters.")
            .Must(v => v is null || v == v.Trim()).WithMessage("Scope must not contain leading or trailing spaces.");
    }
}


public sealed class PatchComplianceRuleRequestValidator: AbstractValidator<PatchComplianceRuleRequest>
{
    private const int NameMaxLen = 150;
    private const int ScopeMaxLen = 20;
    public PatchComplianceRuleRequestValidator()
    {
        RuleFor(x => x).Must(HasAtLeastOneField).WithMessage("At least one field must be provided.");

        RuleFor(x => x.Name)
            .MaximumLength(NameMaxLen).WithMessage($"Name must be at most {NameMaxLen} characters.")
            .Must(v => v is null || v == v.Trim()).WithMessage("Name must not contain leading or trailing spaces.");

        RuleFor(x => x.Scope)
            .MaximumLength(ScopeMaxLen).WithMessage($"Scope must be at most {ScopeMaxLen} characters.")
            .Must(v => v is null || v == v.Trim()).WithMessage("Scope must not contain leading or trailing spaces.")
            .Must(v => v is null || v == "PerClient" || v == "PerAccount").WithMessage("Scope must be one of: PerClient, PerAccount or Null.");


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