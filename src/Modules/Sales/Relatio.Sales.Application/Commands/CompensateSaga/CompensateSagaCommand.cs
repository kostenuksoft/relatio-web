using ErrorOr;
using MediatR;

namespace Relatio.Sales.Application.Commands.CompensateSaga;

public sealed record CompensateSagaCommand(Guid DealId) : IRequest<ErrorOr<Success>>;
