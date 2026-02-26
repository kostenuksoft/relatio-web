namespace Relatio.Sales.Application.DTOs;

public sealed record UpdateDealRequest(
    string Title,
    decimal Amount,
    string Currency,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes);
