using ErrorOr;
using MediatR;
using Relatio.Sales.Domain.Enums;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.CompensateSaga;

public sealed class CompensateSagaCommandHandler : IRequestHandler<CompensateSagaCommand, ErrorOr<Success>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public CompensateSagaCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        CompensateSagaCommand command,
        CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdTrackedAsync(command.DealId, cancellationToken);
        if (deal is null)
            return DealErrors.NotFound;

        deal.CompensateSaga(DealStage.Negotiation);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dealRepository.UpdateAsync(deal, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
