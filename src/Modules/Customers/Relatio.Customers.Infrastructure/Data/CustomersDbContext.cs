using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Relatio.Customers.Domain.Entities;
using Relatio.Shared.Abstractions;

namespace Relatio.Customers.Infrastructure.Data;

public sealed class CustomersDbContext : DbContext, IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("A transaction is already active.");

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No transaction is active.");

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await base.DisposeAsync();
    }
}
