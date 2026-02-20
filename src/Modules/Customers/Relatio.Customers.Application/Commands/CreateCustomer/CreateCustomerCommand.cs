using ErrorOr;
using MediatR;
using Relatio.Customers.Application.DTOs;

namespace Relatio.Customers.Application.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string Email,
    string? Phone,
    string? Industry) : IRequest<ErrorOr<CustomerDto>>;
