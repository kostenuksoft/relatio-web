using ErrorOr;
using MediatR;
using Relatio.Sales.Application.DTOs;

namespace Relatio.Sales.Application.Commands.CreateDeal;

public sealed record CreateDealCommand(
    string Title,
    decimal Amount,
    string Currency,
    Guid CustomerId,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes) : IRequest<ErrorOr<DealDto>>;
