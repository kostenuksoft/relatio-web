using Microsoft.AspNetCore.Identity;

namespace Relatio.Identity.Infrastructure.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7();
    }
}
