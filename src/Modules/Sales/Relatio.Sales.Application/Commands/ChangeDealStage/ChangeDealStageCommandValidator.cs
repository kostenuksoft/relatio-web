using FluentValidation;
using Relatio.Sales.Domain.Enums;

namespace Relatio.Sales.Application.Commands.ChangeDealStage;

public sealed class ChangeDealStageCommandValidator : AbstractValidator<ChangeDealStageCommand>
{
    public ChangeDealStageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Stage)
            .NotEmpty()
            .Must(s => Enum.TryParse<DealStage>(s, true, out _))
            .WithMessage($"Stage must be one of: {string.Join(", ", Enum.GetNames<DealStage>())}.");
    }
}
