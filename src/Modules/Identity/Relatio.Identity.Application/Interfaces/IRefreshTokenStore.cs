using Relatio.Identity.Domain.Models;

namespace Relatio.Identity.Application.Interfaces;

public interface IRefreshTokenStore
{
    Task SaveAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
