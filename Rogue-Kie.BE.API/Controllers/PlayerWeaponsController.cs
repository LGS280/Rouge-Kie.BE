using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller quản lý kho vũ khí của người chơi (Player Weapons Inventory).
    /// Tuân thủ nghiêm ngặt mô hình 3 lớp (API -> Business Service -> Data Access).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerWeaponsController : ControllerBase
    {
        private readonly IPlayerWeaponService _playerWeaponService;

        public PlayerWeaponsController(IPlayerWeaponService playerWeaponService)
        {
            _playerWeaponService = playerWeaponService;
        }

        /// <summary>
        /// Endpoint: GET /api/playerweapons/my-weapons
        /// Lấy danh sách vũ khí đã mở khóa vĩnh viễn của người chơi.
        /// </summary>
        [HttpGet("my-weapons")]
        [Authorize]
        public async Task<IActionResult> GetMyWeapons()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
            }

            var weapons = await _playerWeaponService.GetMyWeaponsAsync(userId);
            return Ok(weapons);
        }

        /// <summary>
        /// Endpoint: POST /api/playerweapons/unlock/{weaponConfigId}
        /// Mở khóa vũ khí theo ID cấu hình.
        /// </summary>
        [HttpPost("unlock/{weaponConfigId}")]
        [Authorize]
        public async Task<IActionResult> UnlockWeapon(int weaponConfigId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
            }

            var result = await _playerWeaponService.UnlockWeaponAsync(userId, weaponConfigId);
            if (result == null)
            {
                return NotFound("Không tìm thấy cấu hình vũ khí với ID tương ứng.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Endpoint: POST /api/playerweapons/dev-reset-my-weapons
        /// Xóa dữ liệu test Kho Vũ Khí và Giao Dịch của người chơi hiện tại.
        /// </summary>
        [HttpPost("dev-reset-my-weapons")]
        [Authorize]
        public async Task<IActionResult> DevResetMyWeapons()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
            }

            await _playerWeaponService.DevResetMyWeaponsAsync(userId);
            return Ok(new { success = true, message = "Đã xóa sạch dữ liệu Kho Vũ Khí và Giao Dịch test của bạn trên DB!" });
        }
    }
}