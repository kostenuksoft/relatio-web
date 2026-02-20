using Serilog;
using Asp.Versioning;
using Scalar.AspNetCore;
using Relatio.Customers.Infrastructure;
using Relatio.Customers.Application;
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

    var app = builder.Build();

    Log.Information("Starting Relatio CRM application");

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseHttpsRedirection();

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
