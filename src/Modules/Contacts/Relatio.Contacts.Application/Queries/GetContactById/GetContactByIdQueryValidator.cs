using FluentValidation;

namespace Relatio.Contacts.Application.Queries.GetContactById;

public sealed class GetContactByIdQueryValidator : AbstractValidator<GetContactByIdQuery>
{
    public GetContactByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
