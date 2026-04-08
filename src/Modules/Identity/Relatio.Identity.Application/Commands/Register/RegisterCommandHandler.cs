using ErrorOr;
using MediatR;
using Relatio.Identity.Application.DTOs;
using Relatio.Identity.Application.Interfaces;
using Relatio.Shared.Enums;

namespace Relatio.Identity.Application.Commands.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<UserResponse>>
{
    private readonly IUserService _userService;

    public RegisterCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ErrorOr<UserResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(
            request.Username,
            request.Email,
            request.Password,
            request.Position,
            UserRole.Viewer,
            request.FirstName,
            request.LastName,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        var principal = result.Value;

        return new UserResponse(
            principal.UserId,
            principal.Username,
            principal.Email,
            principal.Role);
    }
}
