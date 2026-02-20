using ErrorOr;

namespace Relatio.Customers.Domain.Errors;

public static class CustomerErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Customer.NotFound",
        "Customer not found.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Customer.EmailAlreadyExists",
        "Customer with this email already exists.");

    public static readonly Error AlreadyActive = Error.Validation(
        "Customer.AlreadyActive",
        "Customer is already active.");

    public static readonly Error AlreadyInactive = Error.Validation(
        "Customer.AlreadyInactive",
        "Customer is already inactive.");

    public static readonly Error CannotDeactivateProspect = Error.Validation(
        "Customer.CannotDeactivateProspect",
        "Cannot deactivate a prospect customer. Activate first or delete the record.");
}
