using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Application.Models;

namespace Relatio.Identity.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<AuthTokenResponse>>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;

    public LoginCommandHandler(
        IUserService userService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenStore refreshTokenStore)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<ErrorOr<AuthTokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.ValidateCredentialsAsync(
            request.Credential,
            request.Password,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        var principal = result.Value;

        var claims = new TokenClaims(
            principal.UserId,
            principal.Username,
            principal.Email,
            principal.Role);

        var accessTokenResult = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(principal.UserId);

        await _refreshTokenStore.SaveAsync(refreshToken, cancellationToken);

        return new AuthTokenResponse(
            accessTokenResult.Token,
            refreshToken.Token,
            accessTokenResult.ExpiresAt);
    }
}
