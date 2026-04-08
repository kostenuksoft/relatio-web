namespace Relatio.Sales.Application.Messaging;

public sealed record DealStageChangedMessage(
    Guid DealId,
    string Title,
    string PreviousStage,
    string NewStage,
    DateTimeOffset OccurredAt);
