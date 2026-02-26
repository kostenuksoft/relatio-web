using FluentValidation;

namespace Relatio.Contacts.Application.Commands.DeleteContact;

public sealed class DeleteContactCommandValidator : AbstractValidator<DeleteContactCommand>
{
    public DeleteContactCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
