using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.Profile;
using Rogue_Kie.BE.Contracts.Profile;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public PlayerProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
                }

                var profile = await _profileService.GetOrCreateProfileAsync(userId);
                return Ok(new PlayerProfileResponse
                {
                    ProfileId = profile.ProfileId,
                    UserId = profile.UserId,
                    DisplayName = profile.DisplayName,
                    StandardCurrency = profile.StandardCurrency,
                    PremiumCurrency = profile.PremiumCurrency,
                    TotalRuns = profile.TotalRuns,
                    HighestWave = profile.HighestWave,
                    TotalKills = profile.TotalKills,
                    UpdatedAt = profile.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
                }

                var profile = await _profileService.UpdateProfileAsync(userId, request);
                if (profile == null)
                {
                    return NotFound("Không tìm thấy hồ sơ người chơi.");
                }

                return Ok(new PlayerProfileResponse
                {
                    ProfileId = profile.ProfileId,
                    UserId = profile.UserId,
                    DisplayName = profile.DisplayName,
                    StandardCurrency = profile.StandardCurrency,
                    PremiumCurrency = profile.PremiumCurrency,
                    TotalRuns = profile.TotalRuns,
                    HighestWave = profile.HighestWave,
                    TotalKills = profile.TotalKills,
                    UpdatedAt = profile.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
