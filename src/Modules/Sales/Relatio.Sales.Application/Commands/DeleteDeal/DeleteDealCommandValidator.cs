using FluentValidation;

namespace Relatio.Sales.Application.Commands.DeleteDeal;

public sealed class DeleteDealCommandValidator : AbstractValidator<DeleteDealCommand>
{
    public DeleteDealCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
