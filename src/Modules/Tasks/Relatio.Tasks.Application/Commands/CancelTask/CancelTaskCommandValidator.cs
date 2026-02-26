using FluentValidation;

namespace Relatio.Tasks.Application.Commands.CancelTask;

public sealed class CancelTaskCommandValidator : AbstractValidator<CancelTaskCommand>
{
    public CancelTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
