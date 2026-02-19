namespace Relatio.Shared.Abstractions;

public interface ICurrentUserService
{
    Guid UserId { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}
