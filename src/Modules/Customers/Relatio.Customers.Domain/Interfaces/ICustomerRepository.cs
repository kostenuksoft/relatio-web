using Relatio.Customers.Domain.Entities;
using Relatio.Customers.Domain.ValueObjects;
using Relatio.Shared.Abstractions;

namespace Relatio.Customers.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>, IPagedRepository<Customer>
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
}
