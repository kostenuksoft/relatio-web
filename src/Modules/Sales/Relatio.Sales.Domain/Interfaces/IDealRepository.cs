using Relatio.Sales.Domain.Entities;
using Relatio.Sales.Domain.Enums;
using Relatio.Shared.Models;

namespace Relatio.Sales.Domain.Interfaces;

public interface IDealRepository
{
    Task<Deal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Deal?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CursorResult<Deal>> GetPagedAsync(Guid? customerId, DealStage? stage, Guid? cursor, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Deal entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Deal entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Deal entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
