using FluentAssertions;
using Relatio.Identity.Application.Commands.Register;
using Xunit;

namespace Relatio.Identity.Tests.Application;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand Valid(
        string username = "validuser",
        string email = "valid@test.com",
        string password = "Test1234A",
        string confirmPassword = "Test1234A",
        string position = "Developer") =>
        new(username, email, password, confirmPassword, position);

    [Fact]
    public async Task Username_Empty_Fails()
    {
        var result = await _validator.ValidateAsync(Valid(username: ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("user@name")]
    [InlineData("user name")]
    [InlineData("user!")]
    public async Task Username_InvalidFormat_Fails(string username)
    {
        var result = await _validator.ValidateAsync(Valid(username: username));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("valid_user")]
    [InlineData("User123")]
    public async Task Username_Valid_Passes(string username)
    {
        var result = await _validator.ValidateAsync(Valid(username: username));
        result.Errors.Should().NotContain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public async Task Email_Invalid_Fails(string email)
    {
        var result = await _validator.ValidateAsync(Valid(email: email));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("alllowercase1")]
    [InlineData("ALLUPPERCASE1")]
    [InlineData("NoDigitsHere")]
    public async Task Password_WeakRules_Fail(string password)
    {
        var result = await _validator.ValidateAsync(Valid(password: password, confirmPassword: password));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task ConfirmPassword_Mismatch_Fails()
    {
        var result = await _validator.ValidateAsync(Valid(password: "Test1234A", confirmPassword: "Test1234B"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public async Task Position_Empty_Fails()
    {
        var result = await _validator.ValidateAsync(Valid(position: ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Position" && e.ErrorMessage == "Position is required.");
    }

    [Fact]
    public async Task Position_Whitespace_Fails()
    {
        var result = await _validator.ValidateAsync(Valid(position: "   "));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Position");
    }

    [Fact]
    public async Task Position_TooLong_Fails()
    {
        var result = await _validator.ValidateAsync(Valid(position: new string('A', 101)));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Position" && e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public async Task Position_Exactly100Chars_Passes()
    {
        var result = await _validator.ValidateAsync(Valid(position: new string('A', 100)));
        result.Errors.Should().NotContain(e => e.PropertyName == "Position");
    }

    [Fact]
    public async Task AllValid_Passes()
    {
        var result = await _validator.ValidateAsync(Valid());
        result.IsValid.Should().BeTrue();
    }
}
