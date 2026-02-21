using Relatio.Identity.Domain.Models;

namespace Relatio.Identity.Domain.Interfaces;

public interface IUserRepository
{
    Task<UserPrincipal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserPrincipal?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
