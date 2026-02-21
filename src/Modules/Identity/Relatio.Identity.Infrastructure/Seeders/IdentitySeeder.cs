using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relatio.Identity.Infrastructure.Entities;
using Relatio.Shared.Enums;

namespace Relatio.Identity.Infrastructure.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

        var roles = new[] { UserRole.Viewer, UserRole.SalesRep, UserRole.Manager, UserRole.Admin };

        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role.ToString());
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
                logger.LogInformation("Role {Role} created", role);
            }
        }

        var adminEmail = configuration["Identity:AdminEmail"]
            ?? throw new InvalidOperationException("Admin email not configured. Set 'Identity:AdminEmail'.");
        var adminPassword = configuration["Identity:AdminPassword"]
            ?? throw new InvalidOperationException("Admin password not configured. Set 'Identity:AdminPassword'.");

        var adminExists = await userManager.FindByEmailAsync(adminEmail);
        if (adminExists is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
            logger.LogInformation("Admin user created successfully");
        }
        else
        {
            logger.LogInformation("Identity seeding skipped: admin user already exists");
        }
    }
}
