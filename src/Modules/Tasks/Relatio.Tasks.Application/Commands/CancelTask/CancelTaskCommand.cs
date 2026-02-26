using ErrorOr;
using MediatR;

namespace Relatio.Tasks.Application.Commands.CancelTask;

public sealed record CancelTaskCommand(Guid Id) : IRequest<ErrorOr<Success>>;
