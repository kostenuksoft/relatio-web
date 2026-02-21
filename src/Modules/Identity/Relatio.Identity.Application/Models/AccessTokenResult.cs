namespace Relatio.Identity.Application.Models;

public sealed record AccessTokenResult(
    string Token,
    DateTimeOffset ExpiresAt);
