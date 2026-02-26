using FluentValidation;
using Relatio.Tasks.Domain.Enums;

namespace Relatio.Tasks.Application.Queries.GetTasksPaged;

public sealed class GetTasksPagedQueryValidator : AbstractValidator<GetTasksPagedQuery>
{
    public GetTasksPagedQueryValidator()
    {
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status)
            .Must(s => s is null || Enum.TryParse<CrmTaskStatus>(s, true, out _))
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<CrmTaskStatus>())}.");
        RuleFor(x => x.Priority)
            .Must(p => p is null || Enum.TryParse<CrmTaskPriority>(p, true, out _))
            .WithMessage($"Priority must be one of: {string.Join(", ", Enum.GetNames<CrmTaskPriority>())}.");
    }
}
