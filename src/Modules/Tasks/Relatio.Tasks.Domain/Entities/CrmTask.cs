using ErrorOr;
using Relatio.Tasks.Domain.Enums;
using Relatio.Tasks.Domain.Errors;
using Relatio.Tasks.Domain.Events;
using Relatio.Shared.Abstractions;

namespace Relatio.Tasks.Domain.Entities;

public sealed class CrmTask : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public CrmTaskPriority Priority { get; private set; }
    public CrmTaskStatus Status { get; private set; }
    public Guid? AssignedToUserId { get; private set; }

    private CrmTask() { }

    public static ErrorOr<CrmTask> Create(
        string title,
        string? description,
        DateTimeOffset? dueDate,
        CrmTaskPriority priority,
        Guid? assignedToUserId)
    {
        var task = new CrmTask
        {
            Title = title.Trim(),
            Description = description?.Trim(),
            DueDate = dueDate,
            Priority = priority,
            Status = CrmTaskStatus.Pending,
            AssignedToUserId = assignedToUserId
        };

        task.RaiseDomainEvent(new TaskCreatedEvent(
            task.Id,
            task.Title,
            task.Description,
            task.DueDate,
            task.Priority.ToString(),
            task.Status.ToString(),
            task.AssignedToUserId));

        return task;
    }

    public ErrorOr<Success> Update(
        string title,
        string? description,
        DateTimeOffset? dueDate,
        CrmTaskPriority priority,
        Guid? assignedToUserId)
    {
        Title = title.Trim();
        Description = description?.Trim();
        DueDate = dueDate;
        Priority = priority;
        AssignedToUserId = assignedToUserId;
        SetUpdatedAt();

        RaiseDomainEvent(new TaskUpdatedEvent(
            Id,
            Title,
            Description,
            DueDate,
            Priority.ToString(),
            Status.ToString(),
            AssignedToUserId));

        return Result.Success;
    }

    public ErrorOr<Success> Complete()
    {
        if (Status == CrmTaskStatus.Completed)
            return TaskErrors.AlreadyCompleted;

        if (Status == CrmTaskStatus.Cancelled)
            return TaskErrors.AlreadyCancelled;

        Status = CrmTaskStatus.Completed;
        SetUpdatedAt();

        return Result.Success;
    }

    public ErrorOr<Success> Cancel()
    {
        if (Status == CrmTaskStatus.Completed)
            return TaskErrors.AlreadyCompleted;

        if (Status == CrmTaskStatus.Cancelled)
            return TaskErrors.AlreadyCancelled;

        Status = CrmTaskStatus.Cancelled;
        SetUpdatedAt();

        return Result.Success;
    }
}
