namespace Relatio.Sales.Application.Messaging;

public sealed record DealCreatedMessage(
    Guid DealId,
    string Title,
    decimal Amount,
    string Currency,
    string Stage,
    Guid CustomerId,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes,
    DateTimeOffset OccurredAt);
