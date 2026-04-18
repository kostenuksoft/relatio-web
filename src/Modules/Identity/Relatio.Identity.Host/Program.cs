using System.Diagnostics;
using System.Reflection;
using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Relatio.Identity.Application;
using Relatio.Identity.Infrastructure;
using Relatio.Identity.Infrastructure.Data;
using Relatio.Identity.Infrastructure.Seeders;
using Relatio.Shared.Infrastructure;
using Relatio.Shared.Middleware;
using Scalar.AspNetCore;
using Serilog;
using Winton.Extensions.Configuration.Consul;

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
        .AddConsul("relatio/identity", options =>
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

    builder.Services.AddIdentityInfrastructure(builder.Configuration);
    builder.Services.AddIdentityApplication();

    var app = builder.Build();

    Log.Information("Identity service - starting...");

    if (app.Environment.IsDevelopment() ||
        app.Configuration.GetValue<bool>("Identity:RunSeederOnStartup"))
    {
        using var scope = app.Services.CreateScope();
        await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    }

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

    Log.Information("Identity service - ready");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Identity service - startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
