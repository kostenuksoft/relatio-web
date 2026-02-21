namespace Relatio.Identity.Application.DTOs;

public sealed record UserResponse(
    Guid Id,
    string Username,
    string Email,
    string Role);
