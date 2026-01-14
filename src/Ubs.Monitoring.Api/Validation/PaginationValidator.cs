using FluentValidation;
using Ubs.Monitoring.Application.Common.Pagination;

namespace Ubs.Monitoring.Api.Validation;

public abstract class PageRequestValidatorBase<T> : AbstractValidator<T>
{
    protected PageRequestValidatorBase(
        Func<T, int> page,
        Func<T, int> pageSize,
        Func<T, string?> sortBy,
        Func<T, string?> sortDir,
        Func<string, bool> isAllowedSortBy,
        int maxPageSize = PaginationDefaults.MaxPageSize)
    {
        RuleFor(x => page(x)).GreaterThanOrEqualTo(PaginationDefaults.DefaultPage).WithMessage("Page must be >= 1.");

        RuleFor(x => pageSize(x)).InclusiveBetween(1, maxPageSize).WithMessage($"PageSize must be between 1 and {maxPageSize}.");

        RuleFor(x => sortDir(x)).Must(BeNullOrAscDesc).WithMessage("SortDir must be 'asc' or 'desc'.");

        RuleFor(x => sortBy(x))
            .Must(v => string.IsNullOrWhiteSpace(v) || isAllowedSortBy(v.Trim())).WithMessage("SortBy is not allowed for this endpoint.");
    }

    private static bool BeNullOrAscDesc(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;

        var dir = value.Trim();
        return dir.Equals("asc", StringComparison.OrdinalIgnoreCase) || dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
    }
}
