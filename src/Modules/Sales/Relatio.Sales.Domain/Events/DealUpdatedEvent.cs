using Relatio.Shared.Abstractions;

namespace Relatio.Sales.Domain.Events;

public sealed record DealUpdatedEvent(
    Guid DealId,
    string Title,
    decimal Amount,
    string Currency,
    string Stage,
    Guid CustomerId,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes) : IDomainEvent;
