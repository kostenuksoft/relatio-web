using Relatio.Contacts.Domain.Entities;
using Relatio.Shared.Abstractions;
using Relatio.Shared.Models;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Domain.Interfaces;

public interface IContactRepository : IRepository<Contact>
{
    Task<Contact?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailExcludingAsync(Email email, Guid excludeContactId, CancellationToken cancellationToken = default);
    Task<PagedResult<Contact>> GetPagedAsync(
        Guid? customerId,
        string? nameFilter,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
