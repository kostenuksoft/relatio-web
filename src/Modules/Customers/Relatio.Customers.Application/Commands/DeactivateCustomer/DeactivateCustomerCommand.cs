using ErrorOr;
using MediatR;

namespace Relatio.Customers.Application.Commands.DeactivateCustomer;

public sealed record DeactivateCustomerCommand(Guid Id) : IRequest<ErrorOr<Success>>;
