namespace Relatio.Contacts.Application.DTOs;

public sealed record UpdateContactRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Position);
