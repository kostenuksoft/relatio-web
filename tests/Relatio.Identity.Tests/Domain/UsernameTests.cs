using FluentAssertions;
using Relatio.Identity.Domain.ValueObjects;
using Xunit;

namespace Relatio.Identity.Tests.Domain;

public sealed class UsernameTests
{
    [Fact]
    public void Create_WithValidUsername_ShouldSucceed()
    {
        var result = Username.Create("testuser");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("testuser");
    }

    [Fact]
    public void Create_WithValidUsernameUppercase_ShouldNormalizeToLowercase()
    {
        var result = Username.Create("TestUser");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("testuser");
    }

    [Fact]
    public void Create_WithValidUsernameWithUnderscore_ShouldSucceed()
    {
        var result = Username.Create("test_user_123");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("test_user_123");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void Create_WithEmptyUsername_ShouldReturnError(string value)
    {
        var result = Username.Create(value);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Username.Empty");
    }

    [Fact]
    public void Create_WithTooShortUsername_ShouldReturnError()
    {
        var result = Username.Create("ab");

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Username.TooShort");
    }

    [Fact]
    public void Create_WithTooLongUsername_ShouldReturnError()
    {
        var result = Username.Create(new string('a', 31));

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Username.TooLong");
    }

    [Theory]
    [InlineData("test-user")]
    [InlineData("test user")]
    [InlineData("test@user")]
    [InlineData("test.user")]
    public void Create_WithInvalidCharacters_ShouldReturnError(string value)
    {
        var result = Username.Create(value);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Username.InvalidFormat");
    }

    [Fact]
    public void Create_WithMinimumLength_ShouldSucceed()
    {
        var result = Username.Create("abc");

        result.IsError.Should().BeFalse();
        result.Value.Value.Should().Be("abc");
    }

    [Fact]
    public void Create_WithMaximumLength_ShouldSucceed()
    {
        var result = Username.Create(new string('a', 30));

        result.IsError.Should().BeFalse();
        result.Value.Value.Length.Should().Be(30);
    }
}
