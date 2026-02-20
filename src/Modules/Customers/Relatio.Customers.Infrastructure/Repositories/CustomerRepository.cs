using Microsoft.EntityFrameworkCore;
using Relatio.Customers.Domain.Entities;
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

    public async Task<PagedResult<Customer>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking().OrderBy(c => c.Name);

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
}
