using FluentValidation;
using Relatio.Tasks.Domain.Constants;
using Relatio.Tasks.Domain.Enums;

namespace Relatio.Tasks.Application.Commands.UpdateTask;

public sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(TaskConstraints.TitleMaxLength);
        RuleFor(x => x.Description).MaximumLength(TaskConstraints.DescriptionMaxLength).When(x => x.Description is not null);
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<CrmTaskPriority>(p, true, out _))
            .WithMessage($"Priority must be one of: {string.Join(", ", Enum.GetNames<CrmTaskPriority>())}.");
    }
}
