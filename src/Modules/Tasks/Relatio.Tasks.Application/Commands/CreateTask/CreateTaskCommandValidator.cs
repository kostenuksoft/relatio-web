using FluentValidation;
using Relatio.Tasks.Domain.Constants;
using Relatio.Tasks.Domain.Enums;

namespace Relatio.Tasks.Application.Commands.CreateTask;

public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskConstraints.TitleMaxLength);
        RuleFor(x => x.Description).MaximumLength(TaskConstraints.DescriptionMaxLength).When(x => x.Description is not null);
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<CrmTaskPriority>(p, true, out _))
            .WithMessage($"Priority must be one of: {string.Join(", ", Enum.GetNames<CrmTaskPriority>())}.");
    }
}
