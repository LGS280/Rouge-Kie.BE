using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharactersController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharactersController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        /// <summary>
        /// Endpoint: GET /api/characters/my-characters
        /// Lấy danh sách nhân vật và trạng thái mở khóa của người chơi.
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _characterService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _characterService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Create([FromBody] CreateCharacterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _characterService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.CharacterId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCharacterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
 
            var result = await _characterService.UpdateAsync(id, request);
            if (result == null) return NotFound();
 
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _characterService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
