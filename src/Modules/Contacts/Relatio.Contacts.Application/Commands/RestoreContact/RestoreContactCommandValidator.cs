using FluentValidation;

namespace Relatio.Contacts.Application.Commands.RestoreContact;

public sealed class RestoreContactCommandValidator : AbstractValidator<RestoreContactCommand>
{
    public RestoreContactCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
