namespace Relatio.Sales.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public static OutboxMessage Create(string eventType, string payload)
    {
        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            EventType = eventType,
            Payload = payload,
            CreatedAt = DateTimeOffset.UtcNow,
            RetryCount = 0
        };
    }

    public void MarkProcessed()
    {
        ProcessedAt = DateTimeOffset.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error, int maxRetries)
    {
        RetryCount++;
        Error = error;

        if (RetryCount >= maxRetries)
            ProcessedAt = DateTimeOffset.UtcNow;
    }
}
