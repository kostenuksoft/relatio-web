using ErrorOr;
using MediatR;
using Relatio.Sales.Application.DTOs;

namespace Relatio.Sales.Application.Queries.GetDealById;

public sealed record GetDealByIdQuery(Guid Id) : IRequest<ErrorOr<DealDto>>;
