using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Relatio.Infrastructure.Messaging;
using Relatio.Shared.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, _, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.Configure<RabbitMqConsumerSettings>(builder.Configuration.GetSection("RabbitMq"));
    builder.Services.AddHostedService<DealCreatedConsumer>();

    builder.Services.AddHealthChecks()
        .AddCheck("application", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    var app = builder.Build();

    Log.Information("Messaging service - starting...");

    app.UseMiddleware<CorrelationIdMiddleware>();

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Messaging service - startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
