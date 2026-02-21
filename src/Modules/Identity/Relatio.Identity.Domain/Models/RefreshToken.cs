namespace Relatio.Identity.Domain.Models;

public sealed class RefreshToken
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Token { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid UserId { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(string token, DateTimeOffset expiresAt, Guid userId)
    {
        return new RefreshToken
        {
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = userId
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
    }
}
