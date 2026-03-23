using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Relatio.Identity.Api.Requests;
using Relatio.Identity.Application.Commands.Login;
using Relatio.Identity.Application.Commands.RefreshToken;
using Relatio.Identity.Application.Commands.Register;
using Relatio.Shared.Controllers;

namespace Relatio.Identity.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/auth")]
public sealed class AuthController : ApiController
{
    public AuthController(ISender sender) : base(sender)
    {
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Username,
            request.Email,
            request.Password,
            request.ConfirmPassword);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            user => StatusCode(StatusCodes.Status201Created, user),
            HandleErrors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Credential, request.Password);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            Ok,
            HandleErrors);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        var result = await Sender.Send(command, cancellationToken);

        return result.Match(
            Ok,
            HandleErrors);
    }
}
