namespace Relatio.Sales.Application.DTOs;

public sealed record CreateDealRequest(
    string Title,
    decimal Amount,
    string Currency,
    Guid CustomerId,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes);
