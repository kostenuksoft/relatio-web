using ErrorOr;
using MediatR;

namespace Relatio.Contacts.Application.Commands.DeleteContact;

public sealed record DeleteContactCommand(Guid Id) : IRequest<ErrorOr<Success>>;
