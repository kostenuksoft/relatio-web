using Relatio.Shared.Abstractions;

namespace Relatio.Tasks.Domain.Events;

public sealed record TaskCreatedEvent(
    Guid TaskId,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority,
    string Status,
    Guid? AssignedToUserId) : IDomainEvent;
