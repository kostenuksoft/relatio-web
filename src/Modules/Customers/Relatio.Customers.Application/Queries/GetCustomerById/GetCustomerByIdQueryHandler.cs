using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Customers.Application.DTOs;
using Relatio.Customers.Domain.Errors;
using Relatio.Customers.Domain.Interfaces;

namespace Relatio.Customers.Application.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, ErrorOr<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;

    public GetCustomerByIdQueryHandler(
        ICustomerRepository customerRepository,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<CustomerDto>> Handle(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(query.Id, cancellationToken);
        if (customer is null)
            return CustomerErrors.NotFound;

        return _mapper.Map<CustomerDto>(customer);
    }
}
