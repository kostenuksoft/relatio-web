using System.Text.Json;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Relatio.Sales.Application.DTOs;
using Relatio.Sales.Application.Interfaces;
using Relatio.Sales.Application.Messaging;
using Relatio.Sales.Domain.Entities;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Interfaces;

namespace Relatio.Sales.Application.Commands.CreateDeal;

public sealed class CreateDealCommandHandler : IRequestHandler<CreateDealCommand, ErrorOr<DealDto>>
{
    private readonly IDealRepository _dealRepository;
    private readonly ISalesUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly IOutboxRepository _outboxRepository;

    public CreateDealCommandHandler(
        IDealRepository dealRepository,
        ISalesUnitOfWork unitOfWork,
        IMapper mapper,
        ICustomerServiceClient customerServiceClient,
        IOutboxRepository outboxRepository)
    {
        _dealRepository = dealRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _customerServiceClient = customerServiceClient;
        _outboxRepository = outboxRepository;
    }

    public async Task<ErrorOr<DealDto>> Handle(
        CreateDealCommand command,
        CancellationToken cancellationToken)
    {
        var customerExistsResult = await _customerServiceClient.CustomerExistsAsync(
            command.CustomerId, cancellationToken);

        if (customerExistsResult.IsError)
            return DealErrors.CustomerServiceUnavailable;

        if (!customerExistsResult.Value)
            return DealErrors.CustomerNotFound;

        var dealResult = Deal.Create(
            command.Title,
            command.Amount,
            command.Currency,
            command.CustomerId,
            command.ExpectedCloseDate,
            command.Notes);

        if (dealResult.IsError)
            return dealResult.Errors;

        var deal = dealResult.Value;

        var outboxPayload = JsonSerializer.Serialize(new DealCreatedMessage(
            deal.Id,
            deal.Title,
            deal.Amount,
            deal.Currency,
            deal.Stage.ToString(),
            deal.CustomerId,
            deal.ExpectedCloseDate,
            deal.Notes,
            DateTimeOffset.UtcNow));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dealRepository.AddAsync(deal, cancellationToken);
            await _outboxRepository.AddAsync("deal.created", outboxPayload, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return _mapper.Map<DealDto>(deal);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
