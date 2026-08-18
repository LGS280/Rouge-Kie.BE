using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.API.Hubs;
using Rogue_Kie.BE.DataAccess.DBContext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thống kê tổng quan số liệu hệ thống (Tổng User, Số trận đấu, Tổng Gem/Coin, CCU Online)
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                int totalUsers = await _context.Users.CountAsync();
                int totalRuns = await _context.RunHistories.CountAsync();
                long totalGems = await _context.PlayerProfiles.SumAsync(p => (long)p.PremiumCurrency);
                long totalCoins = await _context.PlayerProfiles.SumAsync(p => (long)p.StandardCurrency);
                int onlineCCU = RoomManager.GetCCU();

                return Ok(new
                {
                    TotalUsers = totalUsers,
                    TotalRuns = totalRuns,
                    TotalGemsInEconomy = totalGems,
                    TotalCoinsInEconomy = totalCoins,
                    OnlinePlayerCCU = onlineCCU,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy thống kê hệ thống.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy số lượng người chơi đang online thời gian thực (Concurrent Users - CCU)
        /// </summary>
        [HttpGet("ccu")]
        public IActionResult GetOnlineCCU()
        {
            return Ok(new
            {
                OnlinePlayerCCU = RoomManager.GetCCU(),
                ActiveRooms = RoomManager.ActiveRooms.Count,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Khóa tài khoản người chơi (1-Click Lock User)
        /// </summary>
        [HttpPost("users/{id:int}/lock")]
        public async Task<IActionResult> LockUser([FromRoute] int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { Success = false, Message = "Tài khoản không tồn tại." });

                user.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã khóa thành công tài khoản '{user.Username}' (ID: {user.Id}).",
                    IsActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Mở khóa tài khoản người chơi (1-Click Unlock User)
        /// </summary>
        [HttpPost("users/{id:int}/unlock")]
        public async Task<IActionResult> UnlockUser([FromRoute] int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { Success = false, Message = "Tài khoản không tồn tại." });

                user.IsActive = true;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã mở khóa thành công tài khoản '{user.Username}' (ID: {user.Id}).",
                    IsActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}
