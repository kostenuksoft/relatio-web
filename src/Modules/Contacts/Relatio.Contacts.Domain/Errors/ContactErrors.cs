using ErrorOr;

namespace Relatio.Contacts.Domain.Errors;

public static class ContactErrors
{
    public static readonly Error NotFound = Error.NotFound(
        code: "Contact.NotFound",
        description: "Contact not found.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        code: "Contact.EmailAlreadyExists",
        description: "A contact with this email already exists.");

    public static readonly Error AlreadyDeleted = Error.Validation(
        code: "Contact.AlreadyDeleted",
        description: "Contact is already deleted.");

    public static readonly Error NotDeleted = Error.Validation(
        code: "Contact.NotDeleted",
        description: "Contact is not deleted and cannot be restored.");
}
