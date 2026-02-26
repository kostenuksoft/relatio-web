using ErrorOr;
using Relatio.Contacts.Domain.Events;
using Relatio.Shared.Abstractions;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Domain.Entities;

public sealed class Contact : BaseEntity, ISoftDeletable
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public string? Position { get; private set; }
    public Guid CustomerId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    private Contact() { }

    public static ErrorOr<Contact> Create(
        string firstName,
        string lastName,
        Email? email,
        PhoneNumber? phone,
        string? position,
        Guid customerId)
    {
        var contact = new Contact
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email,
            Phone = phone,
            Position = position?.Trim(),
            CustomerId = customerId
        };

        contact.RaiseDomainEvent(new ContactCreatedEvent(
            contact.Id,
            contact.FirstName,
            contact.LastName,
            contact.Email?.Value,
            contact.Phone?.Value,
            contact.Position,
            contact.CustomerId));

        return contact;
    }

    public ErrorOr<Success> Update(
        string firstName,
        string lastName,
        Email? email,
        PhoneNumber? phone,
        string? position)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email;
        Phone = phone;
        Position = position?.Trim();
        SetUpdatedAt();

        RaiseDomainEvent(new ContactUpdatedEvent(
            Id,
            FirstName,
            LastName,
            Email?.Value,
            Phone?.Value,
            Position,
            CustomerId));

        return Result.Success;
    }

    public ErrorOr<Success> Delete()
    {
        if (IsDeleted)
            return Errors.ContactErrors.AlreadyDeleted;

        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        SetUpdatedAt();

        return Result.Success;
    }

    public ErrorOr<Success> Restore()
    {
        if (!IsDeleted)
            return Errors.ContactErrors.NotDeleted;

        IsDeleted = false;
        DeletedAt = null;
        SetUpdatedAt();

        return Result.Success;
    }
}
