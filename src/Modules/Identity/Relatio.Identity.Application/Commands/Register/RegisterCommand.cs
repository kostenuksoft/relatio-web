using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;

namespace Relatio.Identity.Application.Commands.Register;

public sealed record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string ConfirmPassword,
    string Position,
    string? FirstName = null,
    string? LastName = null) : IRequest<ErrorOr<UserResponse>>;
