using Microsoft.AspNetCore.Identity;

namespace Relatio.Identity.Infrastructure.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Position { get; set; } = string.Empty;
}
