using Microsoft.EntityFrameworkCore;
using Relatio.Customers.Domain.Entities;
using Relatio.Customers.Domain.Enums;
using Relatio.Customers.Domain.Interfaces;
using Relatio.Customers.Domain.ValueObjects;
using Relatio.Customers.Infrastructure.Data;
using Relatio.Shared.Models;

namespace Relatio.Customers.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CustomersDbContext _context;

    public CustomerRepository(CustomersDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Customer>> GetPagedAsync(
        string? nameFilter,
        string? statusFilter,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var escaped = nameFilter.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{escaped}%", "\\"));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter)
            && Enum.TryParse<CustomerStatus>(statusFilter, true, out var status))
            query = query.Where(c => c.Status == status);

        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy.ToLowerInvariant() switch
        {
            "email" => isDescending ? query.OrderByDescending(c => c.Email.Value) : query.OrderBy(c => c.Email.Value),
            "status" => isDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "createdat" => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        _context.Customers.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Email.Value == email.Value, cancellationToken);
    }

    public async Task<bool> ExistsByEmailExcludingAsync(Email email, Guid excludeCustomerId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Email.Value == email.Value && c.Id != excludeCustomerId, cancellationToken);
    }
}
