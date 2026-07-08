using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.Profile;
using System;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public LeaderboardController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int top = 20)
        {
            try
            {
                var leaderboard = await _profileService.GetLeaderboardAsync(top);
                return Ok(leaderboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
