namespace Relatio.Identity.Api.Requests;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword,
    string Position,
    string? FirstName = null,
    string? LastName = null);
