using ErrorOr;
using MediatR;

namespace Relatio.Customers.Application.Commands.ActivateCustomer;

public sealed record ActivateCustomerCommand(Guid Id) : IRequest<ErrorOr<Success>>;
