using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Relatio.Gateway;
using Relatio.Gateway.Services;
using Relatio.Gateway.Settings;
using Relatio.Gateway.Transforms;
using Serilog;
using Winton.Extensions.Configuration.Consul;
using Yarp.ReverseProxy.Transforms.Builder;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

var consulUrl = builder.Configuration["Consul:Url"] ?? "http://localhost:8500";

builder.Configuration
    .AddConsul("relatio/default", options =>
    {
        options.ConsulConfigurationOptions = c => c.Address = new Uri(consulUrl);
        options.Optional = true;
        options.ReloadOnChange = true;
        options.PollWaitTime = TimeSpan.FromSeconds(30);
    })
    .AddConsul("relatio/gateway", options =>
    {
        options.ConsulConfigurationOptions = c => c.Address = new Uri(consulUrl);
        options.Optional = true;
        options.ReloadOnChange = true;
        options.PollWaitTime = TimeSpan.FromSeconds(30);
    });

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<RateLimitingSettings>(
    builder.Configuration.GetSection("RateLimiting"));

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string>("gateway", httpContext =>
    {
        var settings = httpContext.RequestServices
            .GetRequiredService<IOptionsMonitor<RateLimitingSettings>>()
            .CurrentValue;

        var partitionKey = httpContext.User.FindFirst("sub")?.Value
                           ?? httpContext.Connection.RemoteIpAddress?.ToString()
                           ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey + settings.RequestsPerWindow,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.RequestsPerWindow,
                Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Rate limit exceeded. Please try again later." }, ct);

        Log.Warning("Rate limit exceeded for {RemoteIp}", ctx.HttpContext.Connection.RemoteIpAddress);
    };
});

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddHttpClient<DashboardComposer>();
builder.Services.AddSingleton<ITransformProvider, SecurityContextTransformProvider>();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseHttpMetrics();

app.MapMetrics();
app.MapHealthChecks("/health/live");
app.MapControllers();
app.MapReverseProxy();

app.Run();
