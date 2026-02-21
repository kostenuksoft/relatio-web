namespace Relatio.Identity.Api.Requests;

public sealed record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword);
