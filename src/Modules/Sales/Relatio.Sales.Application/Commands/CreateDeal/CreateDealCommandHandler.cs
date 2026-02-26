using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Sales.Application.DTOs;
using Relatio.Sales.Domain.Entities;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.CreateDeal;

public sealed class CreateDealCommandHandler : IRequestHandler<CreateDealCommand, ErrorOr<DealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDealCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ErrorOr<DealDto>> Handle(
        CreateDealCommand command,
        CancellationToken cancellationToken)
    {
        var dealResult = Deal.Create(
            command.Title,
            command.Amount,
            command.Currency,
            command.CustomerId,
            command.ExpectedCloseDate,
            command.Notes);

        if (dealResult.IsError)
            return dealResult.Errors;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dealRepository.AddAsync(dealResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _mapper.Map<DealDto>(dealResult.Value);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
