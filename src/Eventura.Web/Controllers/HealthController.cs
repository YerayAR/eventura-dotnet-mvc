using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Eventura.Web.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
        });
    }

    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // Check if application is ready to serve requests
        return Ok(new
        {
            status = "Ready",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        // Check if application is alive
        return Ok(new
        {
            status = "Live",
            timestamp = DateTime.UtcNow
        });
    }
}
