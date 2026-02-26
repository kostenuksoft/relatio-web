using ErrorOr;
using MediatR;

namespace Relatio.Contacts.Application.Commands.UpdateContact;

public sealed record UpdateContactCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Position) : IRequest<ErrorOr<Success>>;
