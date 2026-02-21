namespace Relatio.Identity.Application.Models;

public sealed record TokenClaims(
    Guid UserId,
    string Username,
    string Email,
    string Role);
