using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller quản lý danh sách nhân vật sở hữu của người chơi (Player Characters Inventory).
    /// Hỗ trợ route /api/playercharacters/my-characters tương tự như /api/playerweapons/my-weapons.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerCharactersController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public PlayerCharactersController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        /// <summary>
        /// Endpoint: GET /api/playercharacters/my-characters
        /// </summary>
        [HttpGet("my-characters")]
        [Authorize]
        public async Task<IActionResult> GetMyCharacters()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
            }

            var characters = await _characterService.GetMyCharactersAsync(userId);
            return Ok(characters);
        }
    }
}
