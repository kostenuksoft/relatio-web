using FluentValidation;
using Relatio.Sales.Domain.Constants;

namespace Relatio.Sales.Application.Commands.CreateDeal;

public sealed class CreateDealCommandValidator : AbstractValidator<CreateDealCommand>
{
    public CreateDealCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(DealConstraints.TitleMaxLength);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(DealConstraints.AmountMin);
        RuleFor(x => x.Currency).NotEmpty().Length(DealConstraints.CurrencyMaxLength);
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(DealConstraints.NotesMaxLength).When(x => x.Notes is not null);
    }
}
