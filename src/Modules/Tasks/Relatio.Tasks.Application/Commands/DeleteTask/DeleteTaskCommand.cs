using ErrorOr;
using MediatR;

namespace Relatio.Tasks.Application.Commands.DeleteTask;

public sealed record DeleteTaskCommand(Guid Id) : IRequest<ErrorOr<Success>>;
