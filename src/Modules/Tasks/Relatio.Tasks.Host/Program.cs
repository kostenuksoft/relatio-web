using System.Diagnostics;
using System.Reflection;
using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Relatio.Tasks.Application;
using Relatio.Tasks.Infrastructure;
using Relatio.Shared.Extensions;
using Relatio.Shared.Infrastructure;
using Winton.Extensions.Configuration.Consul;
using Relatio.Shared.Middleware;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var consulUrl = builder.Configuration["Consul:Url"] ?? "http://localhost:8500";
    var consulEnv = builder.Environment.EnvironmentName.ToLowerInvariant();

    builder.Configuration
        .AddConsul("relatio/default", options =>
        {
            options.ConsulConfigurationOptions = c => c.Address = new Uri(consulUrl);
            options.Optional = true;
            options.ReloadOnChange = true;
            options.PollWaitTime = TimeSpan.FromSeconds(30);
        })
        .AddConsul($"relatio/{consulEnv}", options =>
        {
            options.ConsulConfigurationOptions = c => c.Address = new Uri(consulUrl);
            options.Optional = true;
            options.ReloadOnChange = true;
            options.PollWaitTime = TimeSpan.FromSeconds(30);
        })
        .AddConsul("relatio/tasks", options =>
        {
            options.ConsulConfigurationOptions = c => c.Address = new Uri(consulUrl);
            options.Optional = true;
            options.ReloadOnChange = true;
            options.PollWaitTime = TimeSpan.FromSeconds(30);
        });

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
        var seqUrl = context.Configuration["Seq:ServerUrl"];
        if (seqUrl is not null) configuration.WriteTo.Seq(seqUrl);
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddControllers();

    var versionString = builder.Configuration["ApiVersioning:DefaultVersion"]
        ?? throw new InvalidOperationException("ApiVersioning:DefaultVersion is not configured.");
    var versionParts = versionString.Split('.');
    var defaultApiVersion = new ApiVersion(int.Parse(versionParts[0]), int.Parse(versionParts[1]));

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = defaultApiVersion;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    }).AddMvc();

    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();

    builder.Services.AddKeycloakAuthentication(builder.Configuration);

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

    builder.Services.AddTasksInfrastructure(builder.Configuration);
    builder.Services.AddTasksApplication();

    var app = builder.Build();

    Log.Information("Tasks service - starting...");

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
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

    Log.Information("Tasks service - ready");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Tasks service - startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
