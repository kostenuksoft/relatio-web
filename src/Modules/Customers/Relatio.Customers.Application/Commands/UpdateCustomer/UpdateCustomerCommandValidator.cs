using FluentValidation;
using Relatio.Customers.Domain.Constants;
using Relatio.Customers.Domain.ValueObjects;

namespace Relatio.Customers.Application.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CustomerConstraints.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(CustomerConstraints.EmailMaxLength)
            .Must(email => !Email.Create(email).IsError)
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.Phone)
            .MaximumLength(CustomerConstraints.PhoneMaxLength)
            .Must(phone => !PhoneNumber.Create(phone!).IsError)
            .WithMessage("Phone number must be in E.164 format (e.g., +1234567890).")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Industry)
            .MaximumLength(CustomerConstraints.IndustryMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Industry));
    }
}
