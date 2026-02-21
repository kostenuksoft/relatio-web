using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;

namespace Relatio.Identity.Application.Commands.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword) : IRequest<ErrorOr<UserResponse>>;
