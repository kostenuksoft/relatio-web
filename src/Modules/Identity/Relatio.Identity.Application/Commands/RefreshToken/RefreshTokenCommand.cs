using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;

namespace Relatio.Identity.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<ErrorOr<AuthTokenResponse>>;
