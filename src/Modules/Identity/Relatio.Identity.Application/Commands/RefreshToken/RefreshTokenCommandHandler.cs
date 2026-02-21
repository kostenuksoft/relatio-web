using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;
using Relatio.Identity.Application.Errors;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Application.Models;
using Relatio.Identity.Domain.Interfaces;
using Relatio.Shared.Abstractions;

namespace Relatio.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthTokenResponse>>
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IRefreshTokenStore refreshTokenStore,
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenStore = refreshTokenStore;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<AuthTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingToken = await _refreshTokenStore.GetByTokenAsync(request.RefreshToken, cancellationToken);

            if (existingToken is null || existingToken.IsExpired || existingToken.IsRevoked)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return IdentityErrors.InvalidRefreshToken;
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);

            if (user is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return IdentityErrors.UserNotFound;
            }

            var claims = new TokenClaims(
                user.UserId,
                user.Username,
                user.Email,
                user.Role);

            var accessTokenResult = _jwtTokenService.GenerateAccessToken(claims);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken(existingToken.UserId);

            await _refreshTokenStore.RevokeAsync(request.RefreshToken, cancellationToken);
            await _refreshTokenStore.SaveAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new AuthTokenResponse(
                accessTokenResult.Token,
                newRefreshToken.Token,
                accessTokenResult.ExpiresAt);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
