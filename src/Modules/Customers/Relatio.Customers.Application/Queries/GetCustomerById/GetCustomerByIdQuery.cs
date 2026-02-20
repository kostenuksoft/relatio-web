using ErrorOr;
using MediatR;
using Relatio.Customers.Application.DTOs;

namespace Relatio.Customers.Application.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<ErrorOr<CustomerDto>>;
