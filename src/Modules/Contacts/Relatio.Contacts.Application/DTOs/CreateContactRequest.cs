namespace Relatio.Contacts.Application.DTOs;

public sealed record CreateContactRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Position,
    Guid CustomerId);
