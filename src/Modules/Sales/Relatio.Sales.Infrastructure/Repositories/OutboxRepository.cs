using Relatio.Sales.Application.Interfaces;
using Relatio.Sales.Infrastructure.Data;
using Relatio.Sales.Infrastructure.Outbox;

namespace Relatio.Sales.Infrastructure.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly SalesDbContext _context;

    public OutboxRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(eventType, payload);
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
