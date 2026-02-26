using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Contacts.Application.DTOs;
using Relatio.Contacts.Domain.Interfaces;
using Relatio.Shared.Models;

namespace Relatio.Contacts.Application.Queries.GetContactsPaged;

public sealed class GetContactsPagedQueryHandler : IRequestHandler<GetContactsPagedQuery, ErrorOr<PagedResult<ContactDto>>>
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;

    public GetContactsPagedQueryHandler(
        IContactRepository contactRepository,
        IMapper mapper)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<PagedResult<ContactDto>>> Handle(
        GetContactsPagedQuery query,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _contactRepository.GetPagedAsync(
            query.CustomerId,
            query.Name,
            query.SortBy,
            query.SortDirection,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = _mapper.Map<List<ContactDto>>(pagedResult.Items);

        return new PagedResult<ContactDto>
        {
            Items = items,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
