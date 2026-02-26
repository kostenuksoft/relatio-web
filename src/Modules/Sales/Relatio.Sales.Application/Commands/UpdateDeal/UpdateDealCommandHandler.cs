using ErrorOr;
using MediatR;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.UpdateDeal;

public sealed class UpdateDealCommandHandler : IRequestHandler<UpdateDealCommand, ErrorOr<Success>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;

    public UpdateDealCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        UpdateDealCommand command,
        CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (deal is null)
            return DealErrors.NotFound;

        var updateResult = deal.Update(command.Title, command.Amount, command.Currency, command.ExpectedCloseDate, command.Notes);
        if (updateResult.IsError)
            return updateResult.Errors;

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
