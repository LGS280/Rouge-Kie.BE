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
    /// <summary>
    /// Controller Quản trị chuyên dụng dành riêng cho Ban Quản Trị (Admin & Developer).
    /// Cung cấp các API Thống kê số liệu hệ thống, Theo dõi CCU thời gian thực và Khóa/Mở khóa tài khoản.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Khởi tạo Controller với Dependency Injection AppDbContext
        /// </summary>
        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Endpoint: GET /api/admin/stats
        /// Thống kê tổng quan chỉ số toàn hệ thống:
        /// - Tổng số tài khoản đăng ký (TotalUsers)
        /// - Tổng số lượt chơi đã diễn ra (TotalRuns)
        /// - Tổng kinh tế Gem lưu thông (TotalGemsInEconomy - PremiumCurrency)
        /// - Tổng kinh tế Coin lưu thông (TotalCoinsInEconomy - StandardCurrency)
        /// - Số người chơi đang Online thời gian thực (OnlinePlayerCCU từ SignalR RoomManager)
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
        /// Endpoint: GET /api/admin/ccu
        /// Lấy thông số người chơi đang online thời gian thực (Concurrent Users - CCU)
        /// Trả về số lượng kết nối SignalR đang active và số phòng chơi Co-op đang diễn ra.
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
        /// Endpoint: POST /api/admin/users/{id}/lock
        /// Khóa tài khoản người chơi (1-Click Lock Account).
        /// Cập nhật IsActive = false trong Database Neon PostgreSQL.
        /// Khi IsActive = false, người chơi sẽ bị dập tắt quyền đăng nhập và ngắt kết nối Co-op lập tức.
        /// </summary>
        [HttpPost("users/{id:int}/lock")]
        public async Task<IActionResult> LockUser([FromRoute] int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { Success = false, Message = "Tài khoản không tồn tại." });

                // Chuyển cờ IsActive sang false để khóa tài khoản
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
        /// Endpoint: POST /api/admin/users/{id}/unlock
        /// Mở khóa tài khoản người chơi (1-Click Unlock Account).
        /// Cập nhật IsActive = true trong Database Neon PostgreSQL.
        /// Khôi phục lại quyền đăng nhập và tham gia chơi game bình thường cho người dùng.
        /// </summary>
        [HttpPost("users/{id:int}/unlock")]
        public async Task<IActionResult> UnlockUser([FromRoute] int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { Success = false, Message = "Tài khoản không tồn tại." });

                // Chuyển cờ IsActive sang true để mở khóa tài khoản
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
