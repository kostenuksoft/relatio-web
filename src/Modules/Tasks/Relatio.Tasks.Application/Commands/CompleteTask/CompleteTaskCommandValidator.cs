using FluentValidation;

namespace Relatio.Tasks.Application.Commands.CompleteTask;

public sealed class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
