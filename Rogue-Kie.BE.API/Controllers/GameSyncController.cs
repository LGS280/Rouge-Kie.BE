using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameSyncController : ControllerBase
    {
        private readonly IGameSyncService _gameSyncService;

        public GameSyncController(IGameSyncService gameSyncService)
        {
            _gameSyncService = gameSyncService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSyncData()
        {
            var result = await _gameSyncService.GetSyncDataAsync();
            return Ok(result);
        }
    }
}

