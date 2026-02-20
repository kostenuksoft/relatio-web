using FluentValidation;

namespace Relatio.Customers.Application.Commands.ActivateCustomer;

public sealed class ActivateCustomerCommandValidator : AbstractValidator<ActivateCustomerCommand>
{
    public ActivateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
