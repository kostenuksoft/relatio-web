using Microsoft.EntityFrameworkCore;
using Relatio.Tasks.Domain.Entities;
using Relatio.Tasks.Domain.Enums;
using Relatio.Tasks.Domain.Interfaces;
using Relatio.Tasks.Infrastructure.Data;
using Relatio.Shared.Models;

namespace Relatio.Tasks.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _context;

    public TaskRepository(TasksDbContext context)
    {
        _context = context;
    }

    public async Task<CrmTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<CrmTask?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<CursorResult<CrmTask>> GetPagedAsync(
        CrmTaskStatus? status,
        CrmTaskPriority? priority,
        Guid? assignedToUserId,
        Guid? cursor,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks.AsNoTracking();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (assignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);

        if (cursor.HasValue)
            query = query.Where(t => t.Id > cursor.Value);

        query = query.OrderBy(t => t.Id);

        var items = await query.Take(pageSize + 1).ToListAsync(cancellationToken);

        Guid? nextCursor = null;
        if (items.Count > pageSize)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].Id;
        }

        return new CursorResult<CrmTask>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task AddAsync(CrmTask entity, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(CrmTask entity, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CrmTask entity, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .AnyAsync(t => t.Id == id, cancellationToken);
    }
}
