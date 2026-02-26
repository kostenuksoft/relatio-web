using ErrorOr;
using MediatR;
using Relatio.Contacts.Application.DTOs;

namespace Relatio.Contacts.Application.Queries.GetContactById;

public sealed record GetContactByIdQuery(Guid Id) : IRequest<ErrorOr<ContactDto>>;
