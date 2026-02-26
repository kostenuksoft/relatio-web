using FluentValidation;
using Relatio.Sales.Domain.Enums;

namespace Relatio.Sales.Application.Queries.GetDealsPaged;

public sealed class GetDealsPagedQueryValidator : AbstractValidator<GetDealsPagedQuery>
{
    public GetDealsPagedQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Stage)
            .Must(s => s is null || Enum.TryParse<DealStage>(s, true, out _))
            .WithMessage($"Stage must be one of: {string.Join(", ", Enum.GetNames<DealStage>())}.");
    }
}
