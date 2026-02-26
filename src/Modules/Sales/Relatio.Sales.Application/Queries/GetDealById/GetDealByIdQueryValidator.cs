using FluentValidation;

namespace Relatio.Sales.Application.Queries.GetDealById;

public sealed class GetDealByIdQueryValidator : AbstractValidator<GetDealByIdQuery>
{
    public GetDealByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
