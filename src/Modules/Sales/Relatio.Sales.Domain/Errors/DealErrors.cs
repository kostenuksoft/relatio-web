using ErrorOr;

namespace Relatio.Sales.Domain.Errors;

public static class DealErrors
{
    public static readonly Error NotFound = Error.NotFound(
        code: "Deal.NotFound",
        description: "Deal not found.");

    public static readonly Error AlreadyClosed = Error.Conflict(
        code: "Deal.AlreadyClosed",
        description: "Deal is already closed.");

    public static readonly Error AlreadyDeleted = Error.Conflict(
        code: "Deal.AlreadyDeleted",
        description: "Deal is already deleted.");
}
