using ErrorOr;
using Relatio.Customers.Domain.Enums;
using Relatio.Customers.Domain.Events;
using Relatio.Shared.Abstractions;
using Relatio.Shared.ValueObjects;

namespace Relatio.Customers.Domain.Entities;

public sealed class Customer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public PhoneNumber? Phone { get; private set; }
    public string? Industry { get; private set; }
    public CustomerStatus Status { get; private set; }

    private Customer() { }

    public static ErrorOr<Customer> Create(
        string name,
        Email email,
        PhoneNumber? phone,
        string? industry)
    {
        var customer = new Customer
        {
            Name = name.Trim(),
            Email = email,
            Phone = phone,
            Industry = industry?.Trim(),
            Status = CustomerStatus.Prospect
        };

        customer.RaiseDomainEvent(new CustomerCreatedEvent(
            customer.Id,
            customer.Name,
            customer.Email.Value,
            customer.Phone?.Value,
            customer.Industry,
            customer.Status.ToString()));

        return customer;
    }

    public ErrorOr<Success> Update(
        string name,
        Email email,
        PhoneNumber? phone,
        string? industry)
    {
        Name = name.Trim();
        Email = email;
        Phone = phone;
        Industry = industry?.Trim();
        SetUpdatedAt();

        RaiseDomainEvent(new CustomerUpdatedEvent(
            Id,
            Name,
            Email.Value,
            Phone?.Value,
            Industry,
            Status.ToString()));

        return Result.Success;
    }

    public ErrorOr<Success> Activate()
    {
        if (Status == CustomerStatus.Active)
            return Errors.CustomerErrors.AlreadyActive;

        Status = CustomerStatus.Active;
        SetUpdatedAt();

        return Result.Success;
    }

    public ErrorOr<Success> Deactivate()
    {
        if (Status == CustomerStatus.Prospect)
            return Errors.CustomerErrors.CannotDeactivateProspect;

        if (Status == CustomerStatus.Inactive)
            return Errors.CustomerErrors.AlreadyInactive;

        Status = CustomerStatus.Inactive;
        SetUpdatedAt();

        return Result.Success;
    }
}
