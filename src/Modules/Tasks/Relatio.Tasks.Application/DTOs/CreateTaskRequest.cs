namespace Relatio.Tasks.Application.DTOs;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority,
    Guid? AssignedToUserId);
