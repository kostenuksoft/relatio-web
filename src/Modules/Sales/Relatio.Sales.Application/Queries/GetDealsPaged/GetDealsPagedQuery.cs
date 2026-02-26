using ErrorOr;
using MediatR;
using Relatio.Sales.Application.DTOs;
using Relatio.Shared.Models;

namespace Relatio.Sales.Application.Queries.GetDealsPaged;

public sealed record GetDealsPagedQuery(
    Guid? CustomerId = null,
    string? Stage = null,
    Guid? Cursor = null,
    int PageSize = 20) : IRequest<ErrorOr<CursorResult<DealDto>>>;
