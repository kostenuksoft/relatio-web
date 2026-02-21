using ErrorOr;

namespace Relatio.Identity.Application.Errors;

public static class IdentityErrors
{
    public static class Codes
    {
        public const string InvalidCredentials = "Identity.InvalidCredentials";
        public const string UserAlreadyExists = "Identity.UserAlreadyExists";
        public const string UserNotFound = "Identity.UserNotFound";
        public const string InvalidRefreshToken = "Identity.InvalidRefreshToken";
        public const string RefreshTokenExpired = "Identity.RefreshTokenExpired";
        public const string RegistrationFailed = "Identity.RegistrationFailed";
    }

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        code: Codes.InvalidCredentials,
        description: "Invalid username or password.");

    public static readonly Error UserAlreadyExists = Error.Conflict(
        code: Codes.UserAlreadyExists,
        description: "A user with this username or email already exists.");

    public static readonly Error UserNotFound = Error.NotFound(
        code: Codes.UserNotFound,
        description: "User not found.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        code: Codes.InvalidRefreshToken,
        description: "Invalid refresh token.");

    public static readonly Error RefreshTokenExpired = Error.Unauthorized(
        code: Codes.RefreshTokenExpired,
        description: "Refresh token has expired.");

    public static Error RegistrationFailed(string reason) => Error.Failure(
        code: Codes.RegistrationFailed,
        description: reason);
}
