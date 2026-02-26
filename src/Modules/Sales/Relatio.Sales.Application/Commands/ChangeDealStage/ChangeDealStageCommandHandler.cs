using ErrorOr;
using MediatR;
using Relatio.Sales.Domain.Enums;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.ChangeDealStage;

public sealed class ChangeDealStageCommandHandler : IRequestHandler<ChangeDealStageCommand, ErrorOr<Success>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public ChangeDealStageCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        ChangeDealStageCommand command,
        CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (deal is null)
            return DealErrors.NotFound;

        var stage = Enum.Parse<DealStage>(command.Stage, true);
        var stageResult = deal.ChangeStage(stage);
        if (stageResult.IsError)
            return stageResult.Errors;

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
