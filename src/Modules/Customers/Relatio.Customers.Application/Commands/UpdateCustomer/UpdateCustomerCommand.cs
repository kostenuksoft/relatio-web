using ErrorOr;
using MediatR;

namespace Relatio.Customers.Application.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Industry) : IRequest<ErrorOr<Success>>;
