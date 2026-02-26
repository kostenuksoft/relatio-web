using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Contacts.Application.DTOs;
using Relatio.Contacts.Domain.Errors;
using Relatio.Contacts.Domain.Interfaces;

namespace Relatio.Contacts.Application.Queries.GetContactById;

public sealed class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ErrorOr<ContactDto>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;

    public GetContactByIdQueryHandler(
        IContactRepository contactRepository,
        IMapper mapper)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<ContactDto>> Handle(
        GetContactByIdQuery query,
        CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(query.Id, cancellationToken);
        if (contact is null)
            return ContactErrors.NotFound;

        return _mapper.Map<ContactDto>(contact);
    }
}
