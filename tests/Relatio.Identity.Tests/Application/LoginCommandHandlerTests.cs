using FluentAssertions;
using NSubstitute;
using Relatio.Identity.Application.Commands.Login;
using Relatio.Identity.Application.Errors;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Application.Models;
using Relatio.Identity.Domain.Models;
using Xunit;

namespace Relatio.Identity.Tests.Application;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userService = Substitute.For<IUserService>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _refreshTokenStore = Substitute.For<IRefreshTokenStore>();
        _handler = new LoginCommandHandler(_userService, _jwtTokenService, _refreshTokenStore);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthTokenResponse()
    {
        var command = new LoginCommand("testuser", "Test1234@");
        var userId = Guid.NewGuid();
        var principal = new UserPrincipal(userId, "testuser", "test@test.com", "Viewer");
        var accessToken = new AccessTokenResult("access-token-jwt", DateTimeOffset.UtcNow.AddMinutes(60));
        var refreshToken = RefreshToken.Create("refresh-token", DateTimeOffset.UtcNow.AddDays(7), userId);

        _userService.ValidateCredentialsAsync("testuser", "Test1234@", Arg.Any<CancellationToken>())
            .Returns(principal);
        _jwtTokenService.GenerateAccessToken(Arg.Any<TokenClaims>()).Returns(accessToken);
        _jwtTokenService.GenerateRefreshToken(userId).Returns(refreshToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().Be("access-token-jwt");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.ExpiresAt.Should().BeCloseTo(accessToken.ExpiresAt, TimeSpan.FromSeconds(1));

        await _refreshTokenStore.Received(1).SaveAsync(refreshToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnInvalidCredentialsError()
    {
        var command = new LoginCommand("testuser", "WrongPassword");
        _userService.ValidateCredentialsAsync("testuser", "WrongPassword", Arg.Any<CancellationToken>())
            .Returns(IdentityErrors.InvalidCredentials);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(IdentityErrors.InvalidCredentials);

        _jwtTokenService.DidNotReceive().GenerateAccessToken(Arg.Any<TokenClaims>());
        _ = _refreshTokenStore.DidNotReceive().SaveAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokensWithCorrectClaims()
    {
        var command = new LoginCommand("testuser", "Test1234@");
        var userId = Guid.NewGuid();
        var principal = new UserPrincipal(userId, "testuser", "test@test.com", "Admin");
        var accessToken = new AccessTokenResult("token", DateTimeOffset.UtcNow.AddMinutes(60));
        var refreshToken = RefreshToken.Create("refresh", DateTimeOffset.UtcNow.AddDays(7), userId);

        _userService.ValidateCredentialsAsync("testuser", "Test1234@", Arg.Any<CancellationToken>())
            .Returns(principal);
        _jwtTokenService.GenerateAccessToken(Arg.Any<TokenClaims>()).Returns(accessToken);
        _jwtTokenService.GenerateRefreshToken(userId).Returns(refreshToken);

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.Received(1).GenerateAccessToken(
            Arg.Is<TokenClaims>(c =>
                c.UserId == userId &&
                c.Username == "testuser" &&
                c.Email == "test@test.com" &&
                c.Role == "Admin"));
    }
}
