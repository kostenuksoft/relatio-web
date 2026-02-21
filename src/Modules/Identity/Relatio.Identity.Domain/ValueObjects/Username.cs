using ErrorOr;
using System.Text.RegularExpressions;

namespace Relatio.Identity.Domain.ValueObjects;

public sealed partial record Username
{
    private const int MinLength = 3;
    private const int MaxLength = 30;

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_]+$")]
    private static partial Regex UsernameRegex();

    public static ErrorOr<Username> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(
                code: "Username.Empty",
                description: "Username cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length < MinLength)
            return Error.Validation(
                code: "Username.TooShort",
                description: $"Username must be at least {MinLength} characters long.");

        if (normalized.Length > MaxLength)
            return Error.Validation(
                code: "Username.TooLong",
                description: $"Username must not exceed {MaxLength} characters.");

        if (!UsernameRegex().IsMatch(normalized))
            return Error.Validation(
                code: "Username.InvalidFormat",
                description: "Username can only contain letters, numbers, and underscores.");

        return new Username(normalized);
    }
}
