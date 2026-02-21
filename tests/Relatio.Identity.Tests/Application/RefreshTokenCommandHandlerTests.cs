using FluentAssertions;
using NSubstitute;
using Relatio.Identity.Application.Commands.RefreshToken;
using Relatio.Identity.Application.Errors;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Application.Models;
using Relatio.Identity.Domain.Interfaces;
using Relatio.Identity.Domain.Models;
using Relatio.Shared.Abstractions;
using Xunit;

namespace Relatio.Identity.Tests.Application;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _refreshTokenStore = Substitute.For<IRefreshTokenStore>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RefreshTokenCommandHandler(
            _refreshTokenStore,
            _jwtTokenService,
            _userRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewAuthTokenResponse()
    {
        var userId = Guid.NewGuid();
        var command = new RefreshTokenCommand("valid-refresh-token");
        var existingToken = RefreshToken.Create("valid-refresh-token", DateTimeOffset.UtcNow.AddDays(7), userId);
        var user = new UserPrincipal(userId, "testuser", "test@test.com", "Viewer");
        var newAccessToken = new AccessTokenResult("new-access-token", DateTimeOffset.UtcNow.AddMinutes(60));
        var newRefreshToken = RefreshToken.Create("new-refresh-token", DateTimeOffset.UtcNow.AddDays(7), userId);

        _refreshTokenStore.GetByTokenAsync("valid-refresh-token", Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);
        _jwtTokenService.GenerateAccessToken(Arg.Any<TokenClaims>()).Returns(newAccessToken);
        _jwtTokenService.GenerateRefreshToken(userId).Returns(newRefreshToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");

        await _refreshTokenStore.Received(1).RevokeAsync("valid-refresh-token", Arg.Any<CancellationToken>());
        await _refreshTokenStore.Received(1).SaveAsync(newRefreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnInvalidRefreshTokenError()
    {
        var command = new RefreshTokenCommand("invalid-token");
        _refreshTokenStore.GetByTokenAsync("invalid-token", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(IdentityErrors.InvalidRefreshToken);

        _jwtTokenService.DidNotReceive().GenerateAccessToken(Arg.Any<TokenClaims>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldReturnInvalidRefreshTokenError()
    {
        var userId = Guid.NewGuid();
        var command = new RefreshTokenCommand("expired-token");
        var expiredToken = RefreshToken.Create("expired-token", DateTimeOffset.UtcNow.AddDays(-1), userId);

        _refreshTokenStore.GetByTokenAsync("expired-token", Arg.Any<CancellationToken>())
            .Returns(expiredToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(IdentityErrors.InvalidRefreshToken);

        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}
