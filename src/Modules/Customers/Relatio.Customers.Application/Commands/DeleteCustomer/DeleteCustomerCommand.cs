using ErrorOr;
using MediatR;

namespace Relatio.Customers.Application.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid Id) : IRequest<ErrorOr<Success>>;
