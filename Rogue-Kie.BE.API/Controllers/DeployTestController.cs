using Microsoft.AspNetCore.Mvc;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeployTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Deploy success 🚀",
                time = DateTime.UtcNow
            });
        }

        [HttpGet("ping1111111111")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        [HttpGet("version")]
        public IActionResult Version()
        {
            return Ok(new
            {
                version = "v1.0.0",
                env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}