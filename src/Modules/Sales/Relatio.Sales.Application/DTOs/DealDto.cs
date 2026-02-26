namespace Relatio.Sales.Application.DTOs;

public sealed record DealDto(
    Guid Id,
    string Title,
    decimal Amount,
    string Currency,
    string Stage,
    Guid CustomerId,
    DateTimeOffset? ExpectedCloseDate,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsDeleted);
