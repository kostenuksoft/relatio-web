namespace Relatio.Identity.Domain.Models;

public sealed record UserPrincipal(
    Guid UserId,
    string Username,
    string Email,
    string Role);
