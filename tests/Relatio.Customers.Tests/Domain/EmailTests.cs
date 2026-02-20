using FluentAssertions;
using Relatio.Customers.Domain.ValueObjects;

namespace Relatio.Customers.Tests.Domain;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsEmail()
    {
        var result = Email.Create("test@example.com");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_NormalizesToLowercase()
    {
        var result = Email.Create("Test@EXAMPLE.COM");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = Email.Create("  test@example.com  ");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ReturnsError(string? email)
    {
        var result = Email.Create(email!);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Email.Empty");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test @example.com")]
    [InlineData("test@example")]
    public void Create_WithInvalidFormat_ReturnsError(string email)
    {
        var result = Email.Create(email);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Email.InvalidFormat");
    }

    [Fact]
    public void Emails_WithSameValue_AreEqual()
    {
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("test@example.com").Value;

        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Emails_WithSameCaseInsensitiveValue_AreEqual()
    {
        var email1 = Email.Create("Test@Example.com").Value;
        var email2 = Email.Create("test@example.com").Value;

        email1.Should().Be(email2);
    }

    [Fact]
    public void Create_WithEmailLongerThan254Characters_ReturnsError()
    {
        var longEmail = new string('a', 245) + "@example.com";

        var result = Email.Create(longEmail);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Email.TooLong");
    }
}
