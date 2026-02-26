using FluentValidation;
using Relatio.Sales.Domain.Constants;

namespace Relatio.Sales.Application.Commands.UpdateDeal;

public sealed class UpdateDealCommandValidator : AbstractValidator<UpdateDealCommand>
{
    public UpdateDealCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(DealConstraints.TitleMaxLength);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(DealConstraints.AmountMin);
        RuleFor(x => x.Currency).NotEmpty().Length(DealConstraints.CurrencyMaxLength);
        RuleFor(x => x.Notes).MaximumLength(DealConstraints.NotesMaxLength).When(x => x.Notes is not null);
    }
}
