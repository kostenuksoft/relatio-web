using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Relatio.Identity.Domain.Interfaces;
using Relatio.Identity.Domain.Models;
using Relatio.Identity.Infrastructure.Entities;

namespace Relatio.Identity.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserPrincipal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return new UserPrincipal(user.Id, user.UserName!, user.Email!, role);
    }

    public async Task<UserPrincipal?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(usernameOrEmail)
            ?? await _userManager.FindByEmailAsync(usernameOrEmail);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return new UserPrincipal(user.Id, user.UserName!, user.Email!, role);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is not null;
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user is not null;
    }
}
