using Relatio.Shared.Abstractions;

namespace Relatio.Customers.Domain.Events;

public sealed record CustomerCreatedEvent(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone,
    string? Industry,
    string Status) : IDomainEvent;
