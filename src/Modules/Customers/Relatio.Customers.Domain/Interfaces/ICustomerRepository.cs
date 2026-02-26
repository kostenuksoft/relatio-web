using Relatio.Customers.Domain.Entities;
using Relatio.Shared.ValueObjects;
using Relatio.Shared.Abstractions;
using Relatio.Shared.Models;

namespace Relatio.Customers.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailExcludingAsync(Email email, Guid excludeCustomerId, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Customer>> GetPagedAsync(
        string? nameFilter,
        string? statusFilter,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
