using Serilog;
using Asp.Versioning;
using Scalar.AspNetCore;
using Relatio.Customers.Infrastructure;
using Relatio.Customers.Application;
using Relatio.Identity.Infrastructure;
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
        .AddNpgSql(connectionString);

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

    app.MapHealthChecks("/health");

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
