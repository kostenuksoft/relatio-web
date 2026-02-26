using ErrorOr;
using MediatR;

namespace Relatio.Sales.Application.Commands.UpdateDeal;

public sealed record UpdateDealCommand(
    Guid Id,
    string Title,
    decimal Amount,
    string Currency,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes) : IRequest<ErrorOr<Success>>;
