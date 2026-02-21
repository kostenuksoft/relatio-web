using FluentAssertions;
using NSubstitute;
using Relatio.Identity.Application.Commands.Register;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Application.Errors;
using Relatio.Identity.Domain.Models;
using Relatio.Shared.Enums;
using Xunit;

namespace Relatio.Identity.Tests.Application;

public sealed class RegisterCommandHandlerTests
{
    private readonly IUserService _userService;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _handler = new RegisterCommandHandler(_userService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnUserResponseAndPassViewerRole()
    {
        var command = new RegisterCommand("testuser", "test@test.com", "Test1234@", "Test1234@");
        var expectedPrincipal = new UserPrincipal(Guid.NewGuid(), "testuser", "test@test.com", "Viewer");
        _userService.CreateAsync("testuser", "test@test.com", "Test1234@", UserRole.Viewer, Arg.Any<CancellationToken>())
            .Returns(expectedPrincipal);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Username.Should().Be("testuser");
        result.Value.Email.Should().Be("test@test.com");
        result.Value.Role.Should().Be("Viewer");

        await _userService.Received(1).CreateAsync(
            "testuser",
            "test@test.com",
            "Test1234@",
            UserRole.Viewer,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnUserAlreadyExistsError()
    {
        var command = new RegisterCommand("testuser", "test@test.com", "Test1234@", "Test1234@");
        _userService.CreateAsync("testuser", "test@test.com", "Test1234@", UserRole.Viewer, Arg.Any<CancellationToken>())
            .Returns(IdentityErrors.UserAlreadyExists);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(IdentityErrors.UserAlreadyExists);
    }

    [Fact]
    public async Task Handle_WhenRegistrationFails_ShouldReturnRegistrationFailedError()
    {
        var command = new RegisterCommand("testuser", "test@test.com", "Test1234@", "Test1234@");
        _userService.CreateAsync("testuser", "test@test.com", "Test1234@", UserRole.Viewer, Arg.Any<CancellationToken>())
            .Returns(IdentityErrors.RegistrationFailed("Weak password"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(IdentityErrors.Codes.RegistrationFailed);
    }
}
