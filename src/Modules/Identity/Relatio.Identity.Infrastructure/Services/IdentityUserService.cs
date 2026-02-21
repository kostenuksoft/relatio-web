using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Relatio.Identity.Application.Errors;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Domain.Models;
using Relatio.Identity.Infrastructure.Entities;
using Relatio.Shared.Enums;

namespace Relatio.Identity.Infrastructure.Services;

public sealed class IdentityUserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<UserPrincipal>> CreateAsync(
        string username,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Code switch
            {
                "DuplicateUserName" => IdentityErrors.UserAlreadyExists,
                "DuplicateEmail" => IdentityErrors.UserAlreadyExists,
                _ => IdentityErrors.RegistrationFailed($"{e.Code}: {e.Description}")
            }).ToList();

            return errors;
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role.ToString());

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return IdentityErrors.RegistrationFailed("Failed to assign role.");
        }

        return new UserPrincipal(user.Id, user.UserName!, user.Email!, role.ToString());
    }

    public async Task<ErrorOr<UserPrincipal>> ValidateCredentialsAsync(
        string usernameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(usernameOrEmail)
            ?? await _userManager.FindByEmailAsync(usernameOrEmail);

        if (user is null)
            return IdentityErrors.InvalidCredentials;

        var isValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isValid)
            return IdentityErrors.InvalidCredentials;

        var roles = await _userManager.GetRolesAsync(user);
        var roleString = roles.FirstOrDefault() ?? UserRole.Viewer.ToString();

        return new UserPrincipal(user.Id, user.UserName!, user.Email!, roleString);
    }
}
