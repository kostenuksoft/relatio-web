using Microsoft.EntityFrameworkCore;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Domain.Models;
using Relatio.Identity.Infrastructure.Data;

namespace Relatio.Identity.Infrastructure.Repositories;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AuthDbContext _context;

    public RefreshTokenStore(AuthDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens
            .Where(r => r.Token == token)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.IsRevoked, true)
                    .SetProperty(r => r.RevokedAt, DateTimeOffset.UtcNow),
                cancellationToken);
    }

    public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(r => r.IsRevoked, true)
                    .SetProperty(r => r.RevokedAt, DateTimeOffset.UtcNow),
                cancellationToken);
    }
}
