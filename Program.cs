using System.Diagnostics;
using System.Reflection;
using Serilog;
using Asp.Versioning;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Relatio.Customers.Infrastructure;
using Relatio.Customers.Infrastructure.Data;
using Relatio.Customers.Application;
using Relatio.Identity.Infrastructure;
using Relatio.Identity.Infrastructure.Data;
using Relatio.Identity.Application;
using Relatio.Identity.Infrastructure.Seeders;
using Relatio.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddControllers();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddMvc();

    builder.Services.AddProblemDetails();

    builder.Services.AddOpenApi();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres", tags: ["db", "ready"])
        .AddCheck("application", () => HealthCheckResult.Healthy(
            data: new Dictionary<string, object>
            {
                ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                ["environment"] = builder.Environment.EnvironmentName,
                ["uptime"] = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString(@"dd\.hh\:mm\:ss")
            }), tags: ["ready"]);

    builder.Services.AddCustomersInfrastructure(builder.Configuration);
    builder.Services.AddCustomersApplication();

    builder.Services.AddIdentityInfrastructure(builder.Configuration);
    builder.Services.AddIdentityApplication();

    var app = builder.Build();

    Log.Information("Starting Relatio CRM application");

    if (app.Environment.IsDevelopment() ||
        app.Configuration.GetValue<bool>("Identity:RunSeederOnStartup"))
    {
        using var scope = app.Services.CreateScope();
        await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    }

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
