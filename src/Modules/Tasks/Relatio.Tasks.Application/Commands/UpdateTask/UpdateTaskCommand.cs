using ErrorOr;
using MediatR;

namespace Relatio.Tasks.Application.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority,
    Guid? AssignedToUserId) : IRequest<ErrorOr<Success>>;
