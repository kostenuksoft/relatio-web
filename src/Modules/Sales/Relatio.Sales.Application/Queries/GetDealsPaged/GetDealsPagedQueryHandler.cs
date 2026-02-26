using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Sales.Application.DTOs;
using Relatio.Sales.Domain.Enums;
using Relatio.Sales.Domain.Interfaces;
using Relatio.Shared.Models;

namespace Relatio.Sales.Application.Queries.GetDealsPaged;

public sealed class GetDealsPagedQueryHandler : IRequestHandler<GetDealsPagedQuery, ErrorOr<CursorResult<DealDto>>>
{
    private readonly IDealRepository _dealRepository;
    private readonly IMapper _mapper;

    public GetDealsPagedQueryHandler(
        IDealRepository dealRepository,
        IMapper mapper)
    {
        _dealRepository = dealRepository;
        _mapper = mapper;
    }

    public async Task<ErrorOr<CursorResult<DealDto>>> Handle(
        GetDealsPagedQuery query,
        CancellationToken cancellationToken)
    {
        DealStage? stage = null;
        if (!string.IsNullOrWhiteSpace(query.Stage))
            stage = Enum.Parse<DealStage>(query.Stage, true);

        var cursorResult = await _dealRepository.GetPagedAsync(
            query.CustomerId,
            stage,
            query.Cursor,
            query.PageSize,
            cancellationToken);

        var items = _mapper.Map<List<DealDto>>(cursorResult.Items);

        return new CursorResult<DealDto>
        {
            Items = items,
            NextCursor = cursorResult.NextCursor
        };
    }
}
