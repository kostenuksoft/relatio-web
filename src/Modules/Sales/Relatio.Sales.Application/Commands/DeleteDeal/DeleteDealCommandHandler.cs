using ErrorOr;
using MediatR;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.DeleteDeal;

public sealed class DeleteDealCommandHandler : IRequestHandler<DeleteDealCommand, ErrorOr<Success>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public DeleteDealCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeleteDealCommand command,
        CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (deal is null)
            return DealErrors.NotFound;

        var deleteResult = deal.Delete();
        if (deleteResult.IsError)
            return deleteResult.Errors;

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
