using FluentAssertions;
using Relatio.Customers.Domain.Entities;
using Relatio.Customers.Domain.Enums;
using Relatio.Customers.Domain.Events;
using Relatio.Shared.ValueObjects;

namespace Relatio.Customers.Tests.Domain;

public sealed class CustomerTests
{
    [Fact]
    public void Create_WithValidData_ReturnsCustomer()
    {
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+1234567890").Value;

        var result = Customer.Create("Test Customer", email, phone, "Technology");

        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Test Customer");
        result.Value.Email.Should().Be(email);
        result.Value.Phone.Should().Be(phone);
        result.Value.Industry.Should().Be("Technology");
        result.Value.Status.Should().Be(CustomerStatus.Prospect);
    }

    [Fact]
    public void Create_WithNullPhone_ReturnsCustomer()
    {
        var email = Email.Create("test@example.com").Value;

        var result = Customer.Create("Test Customer", email, null, null);

        result.IsError.Should().BeFalse();
        result.Value.Phone.Should().BeNull();
        result.Value.Industry.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsNameAndIndustry()
    {
        var email = Email.Create("test@example.com").Value;

        var result = Customer.Create("  Test Customer  ", email, null, "  Technology  ");

        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Test Customer");
        result.Value.Industry.Should().Be("Technology");
    }

    [Fact]
    public void Create_RaisesCustomerCreatedEvent()
    {
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("+1234567890").Value;

        var customer = Customer.Create("Test Customer", email, phone, "Technology").Value;

        customer.DomainEvents.Should().HaveCount(1);
        var domainEvent = customer.DomainEvents[0] as CustomerCreatedEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.CustomerId.Should().Be(customer.Id);
        domainEvent.Name.Should().Be("Test Customer");
        domainEvent.Email.Should().Be("test@example.com");
        domainEvent.Phone.Should().Be("+1234567890");
        domainEvent.Industry.Should().Be("Technology");
        domainEvent.Status.Should().Be("Prospect");
    }

    [Fact]
    public void Update_WithValidData_UpdatesCustomer()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;
        customer.ClearDomainEvents();

        var newEmail = Email.Create("new@example.com").Value;
        var newPhone = PhoneNumber.Create("+9876543210").Value;

        var result = customer.Update("Updated Customer", newEmail, newPhone, "Finance");

        result.IsError.Should().BeFalse();
        customer.Name.Should().Be("Updated Customer");
        customer.Email.Should().Be(newEmail);
        customer.Phone.Should().Be(newPhone);
        customer.Industry.Should().Be("Finance");
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_RaisesCustomerUpdatedEvent()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;
        customer.ClearDomainEvents();

        var newEmail = Email.Create("new@example.com").Value;
        customer.Update("Updated Customer", newEmail, null, null);

        customer.DomainEvents.Should().HaveCount(1);
        var domainEvent = customer.DomainEvents[0] as CustomerUpdatedEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.CustomerId.Should().Be(customer.Id);
        domainEvent.Name.Should().Be("Updated Customer");
        domainEvent.Email.Should().Be("new@example.com");
    }

    [Fact]
    public void Activate_FromProspect_SetsStatusToActive()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;

        var result = customer.Activate();

        result.IsError.Should().BeFalse();
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_FromInactive_SetsStatusToActive()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;
        customer.Activate();
        customer.Deactivate();

        var result = customer.Activate();

        result.IsError.Should().BeFalse();
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsError()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;
        customer.Activate();

        var result = customer.Activate();

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Customer.AlreadyActive");
    }

    [Fact]
    public void Deactivate_FromActive_SetsStatusToInactive()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;
        customer.Activate();

        var result = customer.Deactivate();

        result.IsError.Should().BeFalse();
        customer.Status.Should().Be(CustomerStatus.Inactive);
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_FromProspect_ReturnsError()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;

        var result = customer.Deactivate();

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Customer.CannotDeactivateProspect");
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ReturnsError()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;
        customer.Activate();
        customer.Deactivate();

        var result = customer.Deactivate();

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Customer.AlreadyInactive");
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var email = Email.Create("test@example.com").Value;
        var customer = Customer.Create("Test Customer", email, null, null).Value;

        customer.ClearDomainEvents();

        customer.DomainEvents.Should().BeEmpty();
    }
}
