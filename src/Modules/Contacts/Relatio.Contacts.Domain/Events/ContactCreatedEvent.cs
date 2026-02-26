using Relatio.Shared.Abstractions;

namespace Relatio.Contacts.Domain.Events;

public sealed record ContactCreatedEvent(
    Guid ContactId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Position,
    Guid CustomerId) : IDomainEvent;
