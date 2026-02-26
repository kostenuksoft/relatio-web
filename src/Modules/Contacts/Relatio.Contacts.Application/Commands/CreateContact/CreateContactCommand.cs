using ErrorOr;
using MediatR;
using Relatio.Contacts.Application.DTOs;

namespace Relatio.Contacts.Application.Commands.CreateContact;

public sealed record CreateContactCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Position,
    Guid CustomerId) : IRequest<ErrorOr<ContactDto>>;
