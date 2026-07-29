using Microsoft.AspNetCore.Mvc;

namespace Bookify.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class HealthController : ApiController
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    [HttpGet("ready")]
    public IActionResult Readiness()
    {
        // In production, check DB connectivity, Redis, etc.
        return Ok(new { status = "ready" });
    }
}
