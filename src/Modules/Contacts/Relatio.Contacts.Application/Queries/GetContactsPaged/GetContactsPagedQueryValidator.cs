using FluentValidation;

namespace Relatio.Contacts.Application.Queries.GetContactsPaged;

public sealed class GetContactsPagedQueryValidator : AbstractValidator<GetContactsPagedQuery>
{
    public GetContactsPagedQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).Must(v => new[] { "name", "email", "created_at" }.Contains(v))
            .WithMessage("SortBy must be one of: name, email, created_at.");
        RuleFor(x => x.SortDirection).Must(v => new[] { "asc", "desc" }.Contains(v))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
