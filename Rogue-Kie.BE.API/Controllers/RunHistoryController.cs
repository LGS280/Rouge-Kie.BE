using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.RunHistoryService;
using Rogue_Kie.BE.Contracts.RunHistory;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunHistoryController : ControllerBase
    {
        private readonly IRunHistoryService _runHistoryService;

        public RunHistoryController(IRunHistoryService runHistoryService)
        {
            _runHistoryService = runHistoryService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LogRun([FromBody] CreateRunHistoryRequest request)
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

                var run = await _runHistoryService.LogRunHistoryAsync(userId, request);
                return Created($"/api/runhistory/{run.RunId}", new RunHistoryResponse
                {
                    RunId = run.RunId,
                    UserId = run.UserId,
                    CharacterId = run.CharacterId,
                    WavesSurvived = run.WavesSurvived,
                    EnemiesKilled = run.EnemiesKilled,
                    DamageDealt = run.DamageDealt,
                    CurrencyEarned = run.CurrencyEarned,
                    DurationSeconds = run.DurationSeconds,
                    PlayedAt = run.PlayedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserRuns()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
                }

                var runs = await _runHistoryService.GetUserRunsAsync(userId);
                return Ok(runs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
