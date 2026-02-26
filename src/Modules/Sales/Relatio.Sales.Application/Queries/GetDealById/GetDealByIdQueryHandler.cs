using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Sales.Application.DTOs;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Queries.GetDealById;

public sealed class GetDealByIdQueryHandler : IRequestHandler<GetDealByIdQuery, ErrorOr<DealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetDealByIdQueryHandler(
        IDealRepository dealRepository,
        IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<DealDto>> Handle(
        GetDealByIdQuery query,
        CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdAsync(query.Id, cancellationToken);
        if (deal is null)
            return DealErrors.NotFound;

        return _mapper.Map<DealDto>(deal);
    }
}
