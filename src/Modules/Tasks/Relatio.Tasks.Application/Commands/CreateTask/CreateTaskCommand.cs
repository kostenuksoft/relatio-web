using ErrorOr;
using MediatR;
using Relatio.Tasks.Application.DTOs;

namespace Relatio.Tasks.Application.Commands.CreateTask;

public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    DateTimeOffset? DueDate,
    string Priority,
    Guid? AssignedToUserId) : IRequest<ErrorOr<TaskDto>>;
