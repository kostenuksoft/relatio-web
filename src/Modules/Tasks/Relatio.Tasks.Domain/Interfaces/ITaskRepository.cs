using Relatio.Tasks.Domain.Entities;
using Relatio.Tasks.Domain.Enums;
using Relatio.Shared.Models;

namespace Relatio.Tasks.Domain.Interfaces;

public interface ITaskRepository
{
    Task<CrmTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CrmTask?> GetByIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CursorResult<CrmTask>> GetPagedAsync(CrmTaskStatus? status, CrmTaskPriority? priority, Guid? assignedToUserId, Guid? cursor, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(CrmTask entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(CrmTask entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(CrmTask entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
