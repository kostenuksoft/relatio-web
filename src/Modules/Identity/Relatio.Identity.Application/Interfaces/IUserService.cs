using ErrorOr;
using Relatio.Shared.Enums;
using Relatio.Identity.Domain.Models;

namespace Relatio.Identity.Application.Interfaces;

public interface IUserService
{
    Task<ErrorOr<UserPrincipal>> CreateAsync(
        string username,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default);

    Task<ErrorOr<UserPrincipal>> ValidateCredentialsAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default);
}
