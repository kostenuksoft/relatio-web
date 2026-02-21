using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;

namespace Relatio.Identity.Application.Commands.Login;

public sealed record LoginCommand(
    string Credential,
    string Password) : IRequest<ErrorOr<AuthTokenResponse>>;
