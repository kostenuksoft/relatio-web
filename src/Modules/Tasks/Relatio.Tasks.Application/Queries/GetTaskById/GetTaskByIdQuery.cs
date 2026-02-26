using ErrorOr;
using MediatR;
using Relatio.Tasks.Application.DTOs;

namespace Relatio.Tasks.Application.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IRequest<ErrorOr<TaskDto>>;
