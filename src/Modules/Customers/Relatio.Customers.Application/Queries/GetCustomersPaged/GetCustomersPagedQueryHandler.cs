using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Customers.Application.DTOs;
using Relatio.Customers.Domain.Interfaces;
using Relatio.Shared.Models;

namespace Relatio.Customers.Application.Queries.GetCustomersPaged;

public sealed class GetCustomersPagedQueryHandler : IRequestHandler<GetCustomersPagedQuery, ErrorOr<PagedResult<CustomerDto>>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetCustomersPagedQueryHandler(
        ICustomerRepository customerRepository,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<PagedResult<CustomerDto>>> Handle(
        GetCustomersPagedQuery query,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _customerRepository.GetPagedAsync(
            query.Name,
            query.Status,
            query.SortBy,
            query.SortDirection,
            query.Page,
            query.PageSize,
            cancellationToken);

        var items = _mapper.Map<List<CustomerDto>>(pagedResult.Items);

        return new PagedResult<CustomerDto>
        {
            Items = items,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
