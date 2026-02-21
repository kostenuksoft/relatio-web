namespace Relatio.Identity.Api.Requests;

public sealed record LoginRequest(
    string Credential,
    string Password);
