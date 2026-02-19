namespace Relatio.Shared.Abstractions;

public interface IPagedRepository<T> where T : BaseEntity
{
    Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
