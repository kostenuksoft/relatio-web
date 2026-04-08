using System.Text.Json;
using ErrorOr;
using MediatR;
using Relatio.Sales.Application.Interfaces;
using Relatio.Sales.Application.Messaging;
using Relatio.Sales.Domain.Enums;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.ChangeDealStage;

public sealed class ChangeDealStageCommandHandler : IRequestHandler<ChangeDealStageCommand, ErrorOr<Success>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outboxRepository;

    public ChangeDealStageCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork,
        IOutboxRepository outboxRepository)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
    }

    public async Task<ErrorOr<Success>> Handle(
        ChangeDealStageCommand command,
        CancellationToken cancellationToken)
    {
        var deal = await _dealRepository.GetByIdTrackedAsync(command.Id, cancellationToken);
        if (deal is null)
            return DealErrors.NotFound;

        var previousStage = deal.Stage.ToString();
        var newStage = Enum.Parse<DealStage>(command.Stage, true);
        var stageResult = deal.ChangeStage(newStage);
        if (stageResult.IsError)
            return stageResult.Errors;

        var outboxPayload = JsonSerializer.Serialize(new DealStageChangedMessage(
            deal.Id,
            deal.Title,
            previousStage,
            newStage.ToString(),
            DateTimeOffset.UtcNow));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dealRepository.UpdateAsync(deal, cancellationToken);
            await _outboxRepository.AddAsync("deal.stage.changed", outboxPayload, cancellationToken);
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
