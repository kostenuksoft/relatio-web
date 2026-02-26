namespace Relatio.Tasks.Application.DTOs;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority,
    Guid? AssignedToUserId);
