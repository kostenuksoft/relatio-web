using ErrorOr;
using MediatR;

namespace Relatio.Sales.Application.Commands.DeleteDeal;

public sealed record DeleteDealCommand(Guid Id) : IRequest<ErrorOr<Success>>;
