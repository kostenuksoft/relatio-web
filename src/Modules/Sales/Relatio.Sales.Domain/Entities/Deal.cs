using ErrorOr;
using Relatio.Sales.Domain.Enums;
using Relatio.Sales.Domain.Errors;
using Relatio.Sales.Domain.Events;
using Relatio.Shared.Abstractions;

namespace Relatio.Sales.Domain.Entities;

public sealed class Deal : BaseEntity, ISoftDeletable
{
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DealStage Stage { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTimeOffset? ExpectedCloseDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    private Deal() { }

    public static ErrorOr<Deal> Create(
        string title,
        decimal amount,
        string currency,
        Guid customerId,
        DateTimeOffset? expectedCloseDate,
        string? notes)
    {
        var deal = new Deal
        {
            Title = title.Trim(),
            Amount = amount,
            Currency = currency.Trim().ToUpperInvariant(),
            Stage = DealStage.Prospecting,
            CustomerId = customerId,
            ExpectedCloseDate = expectedCloseDate,
            Notes = notes?.Trim()
        };

        deal.RaiseDomainEvent(new DealCreatedEvent(
            deal.Id,
            deal.Title,
            deal.Amount,
            deal.Currency,
            deal.Stage.ToString(),
            deal.CustomerId,
            deal.ExpectedCloseDate,
            deal.Notes));

        return deal;
    }

    public ErrorOr<Success> Update(
        string title,
        decimal amount,
        string currency,
        DateTimeOffset? expectedCloseDate,
        string? notes)
    {
        Title = title.Trim();
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        ExpectedCloseDate = expectedCloseDate;
        Notes = notes?.Trim();
        SetUpdatedAt();

        RaiseDomainEvent(new DealUpdatedEvent(
            Id,
            Title,
            Amount,
            Currency,
            Stage.ToString(),
            CustomerId,
            ExpectedCloseDate,
            Notes));

        return Result.Success;
    }

    public ErrorOr<Success> ChangeStage(DealStage newStage)
    {
        if (Stage == DealStage.ClosedWon || Stage == DealStage.ClosedLost)
            return DealErrors.AlreadyClosed;

        var previousStage = Stage;
        Stage = newStage;
        SetUpdatedAt();

        RaiseDomainEvent(new DealStageChangedEvent(Id, previousStage.ToString(), newStage.ToString()));

        return Result.Success;
    }

    public ErrorOr<Success> Delete()
    {
        if (IsDeleted)
            return DealErrors.AlreadyDeleted;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        SetUpdatedAt();

        return Result.Success;
    }
}
