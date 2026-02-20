using ErrorOr;
using MediatR;
using Relatio.Customers.Application.DTOs;
using Relatio.Shared.Models;

namespace Relatio.Customers.Application.Queries.GetCustomersPaged;

public sealed record GetCustomersPagedQuery(
    string? Name = null,
    string? Status = null,
    string SortBy = "name",
    string SortDirection = "asc",
    int Page = 1,
    int PageSize = 20) : IRequest<ErrorOr<PagedResult<CustomerDto>>>;
