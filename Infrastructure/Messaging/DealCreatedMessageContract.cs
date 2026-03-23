namespace Relatio.Infrastructure.Messaging;

public sealed record DealCreatedMessageContract(
    Guid DealId,
    string Title,
    decimal Amount,
    string Currency,
    string Stage,
    Guid CustomerId,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes,
    DateTimeOffset OccurredAt);
