namespace Relatio.Sales.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(string eventType, string payload, CancellationToken cancellationToken = default);
}
