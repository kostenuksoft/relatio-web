namespace Relatio.Tasks.Application.DTOs;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority,
    string Status,
    Guid? AssignedToUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
