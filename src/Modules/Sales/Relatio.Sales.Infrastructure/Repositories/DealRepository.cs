using Microsoft.EntityFrameworkCore;
using Relatio.Sales.Domain.Entities;
using Relatio.Sales.Domain.Enums;
using Relatio.Sales.Domain.Interfaces;
using Relatio.Sales.Infrastructure.Data;
using Relatio.Shared.Models;

namespace Relatio.Sales.Infrastructure.Repositories;

public sealed class DealRepository : IDealRepository
{
    private readonly SalesDbContext _context;

    public DealRepository(SalesDbContext context)
    {
        _context = context;
    }

    public async Task<Deal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Deal?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<CursorResult<Deal>> GetPagedAsync(
        Guid? customerId,
        DealStage? stage,
        Guid? cursor,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Deals.AsNoTracking();

        if (customerId.HasValue)
            query = query.Where(d => d.CustomerId == customerId.Value);

        if (stage.HasValue)
            query = query.Where(d => d.Stage == stage.Value);

        if (cursor.HasValue)
            query = query.Where(d => d.Id > cursor.Value);

        query = query.OrderBy(d => d.Id);

        var items = await query.Take(pageSize + 1).ToListAsync(cancellationToken);

        Guid? nextCursor = null;
        if (items.Count > pageSize)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].Id;
        }

        return new CursorResult<Deal>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task AddAsync(Deal entity, CancellationToken cancellationToken = default)
    {
        await _context.Deals.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Deal entity, CancellationToken cancellationToken = default)
    {
        _context.Deals.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Deal entity, CancellationToken cancellationToken = default)
    {
        _context.Deals.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .AsNoTracking()
            .AnyAsync(d => d.Id == id, cancellationToken);
    }
}
