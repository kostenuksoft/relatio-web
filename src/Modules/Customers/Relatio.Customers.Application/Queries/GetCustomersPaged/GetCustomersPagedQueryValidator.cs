using FluentValidation;

namespace Relatio.Customers.Application.Queries.GetCustomersPaged;

public sealed class GetCustomersPagedQueryValidator : AbstractValidator<GetCustomersPagedQuery>
{
    private static readonly string[] AllowedSortFields = ["name", "email", "status", "created_at"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetCustomersPagedQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy)
            .NotEmpty()
            .Must(sortBy => AllowedSortFields.Contains(sortBy.ToLowerInvariant()))
            .WithMessage("SortBy must be one of: name, email, status, created_at.");

        RuleFor(x => x.SortDirection)
            .NotEmpty()
            .Must(dir => AllowedSortDirections.Contains(dir.ToLowerInvariant()))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
