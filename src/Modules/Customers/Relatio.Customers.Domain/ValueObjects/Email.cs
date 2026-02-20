using System.Text.RegularExpressions;
using ErrorOr;

namespace Relatio.Customers.Domain.ValueObjects;

public sealed partial record Email
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email CreateUnsafe(string value) => new(value);

    public static ErrorOr<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(code: "Email.Empty", description: "Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 254)
            return Error.Validation(code: "Email.TooLong", description: "Email must not exceed 254 characters.");

        if (!EmailRegex().IsMatch(normalized))
            return Error.Validation(code: "Email.InvalidFormat", description: "Email format is invalid.");

        return new Email(normalized);
    }
}
