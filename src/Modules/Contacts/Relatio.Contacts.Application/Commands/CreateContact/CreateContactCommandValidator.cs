using FluentValidation;
using Relatio.Contacts.Domain.Constants;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Application.Commands.CreateContact;

public sealed class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(ContactConstraints.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(ContactConstraints.LastNameMaxLength);

        RuleFor(x => x.Email)
            .MaximumLength(ContactConstraints.EmailMaxLength)
            .Must(email => !Email.Create(email!).IsError)
            .WithMessage("Email format is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(ContactConstraints.PhoneMaxLength)
            .Must(phone => !PhoneNumber.Create(phone!).IsError)
            .WithMessage("Phone number must be in E.164 format (e.g., +1234567890).")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Position)
            .MaximumLength(ContactConstraints.PositionMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Position));

        RuleFor(x => x.CustomerId)
            .NotEmpty();
    }
}
