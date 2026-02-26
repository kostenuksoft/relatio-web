using ErrorOr;

namespace Relatio.Tasks.Domain.Errors;

public static class TaskErrors
{
    public static readonly Error NotFound = Error.NotFound(
        code: "Task.NotFound",
        description: "Task not found.");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        code: "Task.AlreadyCompleted",
        description: "Task is already completed.");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        code: "Task.AlreadyCancelled",
        description: "Task is already cancelled.");
}
