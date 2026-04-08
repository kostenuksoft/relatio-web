using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Relatio.Identity.Application;
using Relatio.Identity.Application.Commands.Register;
using Relatio.Identity.Application.Interfaces;
using Relatio.Identity.Domain.Models;
using Relatio.Shared.Enums;
using Xunit;

namespace Relatio.Identity.Tests.Application;

public sealed class RegisterPipelineTests
{
    private static ISender BuildSender(IUserService userService)
    {
        var services = new ServiceCollection();
        services.AddIdentityApplication();
        services.AddSingleton(userService);
        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    private static IUserService StubUserService()
    {
        var svc = Substitute.For<IUserService>();
        svc.CreateAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<UserRole>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new UserPrincipal(Guid.NewGuid(), "u", "u@t.com", "Viewer"));
        return svc;
    }

    [Fact]
    public async Task Position_Empty_FailsInPipeline()
    {
        var sender = BuildSender(StubUserService());
        var command = new RegisterCommand("validuser", "valid@test.com", "Test1234A", "Test1234A", "");

        var result = await sender.Send(command);

        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Position" && e.Description == "Position is required.");
    }

    [Fact]
    public async Task Position_Whitespace_FailsInPipeline()
    {
        var sender = BuildSender(StubUserService());
        var command = new RegisterCommand("validuser", "valid@test.com", "Test1234A", "Test1234A", "   ");

        var result = await sender.Send(command);

        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Position");
    }

    [Fact]
    public async Task Password_Empty_FailsInPipeline()
    {
        var sender = BuildSender(StubUserService());
        var command = new RegisterCommand("validuser", "valid@test.com", "", "", "Developer");

        var result = await sender.Send(command);

        result.IsError.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Password");
    }

    [Fact]
    public async Task AllValid_ReachesHandlerAndSucceeds()
    {
        var userService = StubUserService();
        var sender = BuildSender(userService);
        var command = new RegisterCommand("validuser", "valid@test.com", "Test1234A", "Test1234A", "Developer");

        var result = await sender.Send(command);

        result.IsError.Should().BeFalse();
        await userService.Received(1).CreateAsync(
            "validuser", "valid@test.com", "Test1234A", "Developer",
            UserRole.Viewer, null, null, Arg.Any<CancellationToken>());
    }
}
