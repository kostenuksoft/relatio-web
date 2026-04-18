using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Relatio.Gateway.Settings;

namespace Relatio.Gateway.Controllers;

[Route("api/config")]
[ApiController]
public sealed class ConfigController : ControllerBase
{
    private readonly IOptionsMonitor<RateLimitingSettings> _rateLimitingSettings;

    public ConfigController(IOptionsMonitor<RateLimitingSettings> rateLimitingSettings)
    {
        _rateLimitingSettings = rateLimitingSettings;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var settings = _rateLimitingSettings.CurrentValue;

        return Ok(new
        {
            rateLimiting = new
            {
                requestsPerWindow = settings.RequestsPerWindow,
                windowSeconds = settings.WindowSeconds
            },
            source = "consul+appsettings"
        });
    }
}
