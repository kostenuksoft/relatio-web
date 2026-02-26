using FluentAssertions;
using Relatio.Shared.ValueObjects;

namespace Relatio.Customers.Tests.Domain;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+1234567")]
    [InlineData("+380501234567")]
    [InlineData("+123456789012345")]
    public void Create_WithValidE164_ReturnsPhoneNumber(string phone)
    {
        var result = PhoneNumber.Create(phone);

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be(phone);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = PhoneNumber.Create("  +1234567890  ");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPhone_ReturnsError(string? phone)
    {
        var result = PhoneNumber.Create(phone!);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("PhoneNumber.Empty");
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("+0123456789")]
    [InlineData("+12 345 67890")]
    [InlineData("+(123)4567890")]
    [InlineData("+")]
    [InlineData("+1")]
    [InlineData("+12")]
    [InlineData("+123456")]
    [InlineData("+1234567890123456")]
    public void Create_WithInvalidE164Format_ReturnsError(string phone)
    {
        var result = PhoneNumber.Create(phone);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("PhoneNumber.InvalidFormat");
    }

    [Fact]
    public void PhoneNumbers_WithSameValue_AreEqual()
    {
        var phone1 = PhoneNumber.Create("+1234567890").Value;
        var phone2 = PhoneNumber.Create("+1234567890").Value;

        phone1.Should().Be(phone2);
        (phone1 == phone2).Should().BeTrue();
    }
}
