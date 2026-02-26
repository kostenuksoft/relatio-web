namespace Relatio.Contacts.Application.DTOs;

public sealed record ContactDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? Phone,
    string? Position,
    Guid CustomerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsDeleted);
