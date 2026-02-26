using ErrorOr;
using MediatR;

namespace Relatio.Sales.Application.Commands.ChangeDealStage;

public sealed record ChangeDealStageCommand(Guid Id, string Stage) : IRequest<ErrorOr<Success>>;
