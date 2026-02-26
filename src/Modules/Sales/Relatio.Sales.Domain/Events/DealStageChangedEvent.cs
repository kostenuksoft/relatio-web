using Relatio.Shared.Abstractions;

namespace Relatio.Sales.Domain.Events;

public sealed record DealStageChangedEvent(
    Guid DealId,
    string PreviousStage,
    string NewStage) : IDomainEvent;
