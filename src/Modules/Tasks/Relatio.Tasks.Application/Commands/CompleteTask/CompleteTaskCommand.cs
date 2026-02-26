using ErrorOr;
using MediatR;

namespace Relatio.Tasks.Application.Commands.CompleteTask;

public sealed record CompleteTaskCommand(Guid Id) : IRequest<ErrorOr<Success>>;
