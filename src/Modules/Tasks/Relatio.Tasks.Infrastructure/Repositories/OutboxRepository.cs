using Relatio.Tasks.Application.Interfaces;
using Relatio.Tasks.Infrastructure.Data;
using Relatio.Tasks.Infrastructure.Outbox;

namespace Relatio.Tasks.Infrastructure.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly TasksDbContext _context;

    public OutboxRepository(TasksDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(eventType, payload);
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
