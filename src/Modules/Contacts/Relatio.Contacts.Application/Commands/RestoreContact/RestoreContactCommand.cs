using ErrorOr;
using MediatR;

namespace Relatio.Contacts.Application.Commands.RestoreContact;

public sealed record RestoreContactCommand(Guid Id) : IRequest<ErrorOr<Success>>;
