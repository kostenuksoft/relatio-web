using Microsoft.AspNetCore.Mvc;
using Relatio.Gateway.Services;

namespace Relatio.Gateway.Controllers;

[Route("api/dashboard")]
[ApiController]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardComposer _composer;

    public DashboardController(DashboardComposer composer)
    {
        _composer = composer;
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerDashboard(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        var result = await _composer.GetCustomerDashboardAsync(
            customerId,
            authHeader,
            correlationId,
            cancellationToken);

        if (result.Customer == null && result.Unavailable.Contains("customers"))
        {
            return StatusCode(503, new
            {
                error = "Customer service unavailable",
                unavailable = result.Unavailable
            });
        }

        return Ok(result);
    }
}
