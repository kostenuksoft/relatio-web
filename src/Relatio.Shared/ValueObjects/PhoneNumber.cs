using System.Text.RegularExpressions;
using ErrorOr;

namespace Relatio.Shared.ValueObjects;

public sealed partial record PhoneNumber
{
    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Regex();

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber CreateUnsafe(string value) => new(value);

    public static explicit operator string(PhoneNumber phone) => phone.Value;

    public static ErrorOr<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation(code: "PhoneNumber.Empty", description: "Phone number cannot be empty.");

        var normalized = value.Trim();

        if (!E164Regex().IsMatch(normalized))
            return Error.Validation(code: "PhoneNumber.InvalidFormat", description: "Phone number must be in E.164 format (e.g., +1234567890).");

        return new PhoneNumber(normalized);
    }
}
