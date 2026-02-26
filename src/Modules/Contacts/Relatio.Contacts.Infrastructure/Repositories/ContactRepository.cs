using Microsoft.EntityFrameworkCore;
using Relatio.Contacts.Domain.Entities;
using Relatio.Contacts.Domain.Interfaces;
using Relatio.Contacts.Infrastructure.Data;
using Relatio.Shared.Models;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Infrastructure.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly ContactsDbContext _context;

    public ContactRepository(ContactsDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contact?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Contact>> GetPagedAsync(
        Guid? customerId,
        string? nameFilter,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contacts.AsNoTracking();

        if (customerId.HasValue)
            query = query.Where(c => c.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var escaped = nameFilter.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            query = query.Where(c =>
                EF.Functions.ILike(c.FirstName, $"%{escaped}%", "\\") ||
                EF.Functions.ILike(c.LastName, $"%{escaped}%", "\\"));
        }

        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy.ToLowerInvariant() switch
        {
            "email" => isDescending
                ? query.OrderByDescending(c => c.Email)
                : query.OrderBy(c => c.Email),
            "created_at" => isDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            _ => isDescending
                ? query.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName)
                : query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Contact>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Contact entity, CancellationToken cancellationToken = default)
    {
        await _context.Contacts.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Contact entity, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Contact entity, CancellationToken cancellationToken = default)
    {
        _context.Contacts.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailExcludingAsync(Email email, Guid excludeContactId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Email == email && c.Id != excludeContactId, cancellationToken);
    }
}
