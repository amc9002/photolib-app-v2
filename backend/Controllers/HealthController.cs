using Microsoft.AspNetCore.Mvc;

namespace PhotoLibApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { status = "ok", time = System.DateTime.UtcNow });
    }
}
