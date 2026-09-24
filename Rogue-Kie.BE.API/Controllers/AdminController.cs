using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.API.Hubs;
using Rogue_Kie.BE.API.Hubs.Models;
using Rogue_Kie.BE.Contracts.Admin;
using Rogue_Kie.BE.DataAccess.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller Quản trị chuyên dụng dành riêng cho Ban Quản Trị (Admin & Developer).
    /// Cung cấp các API Thống kê số liệu hệ thống, Theo dõi CCU thời gian thực, Giao dịch PayOS và Khóa/Mở khóa tài khoản.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Developer")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<GameHub> _hubContext;

        /// <summary>
        /// Khởi tạo Controller với Dependency Injection AppDbContext và IHubContext<GameHub>
        /// </summary>
        public AdminController(AppDbContext context, IHubContext<GameHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Endpoint: GET /api/admin/stats
        /// Thống kê tổng quan chỉ số toàn hệ thống:
        /// - Tổng số tài khoản đăng ký (TotalUsers)
        /// - Tổng số lượt chơi đã diễn ra (TotalRuns)
        /// - Tổng kinh tế Gem lưu thông (TotalGemsInEconomy - PremiumCurrency)
        /// - Tổng kinh tế Coin lưu thông (TotalCoinsInEconomy - StandardCurrency)
        /// - Số người chơi đang Online thời gian thực (OnlinePlayerCCU từ SignalR RoomManager)
        /// - Số phòng Co-op đang mở (ActiveRooms)
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
                int activeRooms = RoomManager.ActiveRooms.Count;

                return Ok(new
                {
                    TotalUsers = totalUsers,
                    TotalRuns = totalRuns,
                    TotalGemsInEconomy = totalGems,
                    TotalCoinsInEconomy = totalCoins,
                    OnlinePlayerCCU = onlineCCU,
                    ActiveRooms = activeRooms,
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
        /// Trả về số lượng kết nối SignalR đang active, số phòng chơi Co-op và chi tiết các phòng.
        /// </summary>
        [HttpGet("ccu")]
        public IActionResult GetOnlineCCU()
        {
            var roomDetails = RoomManager.ActiveRooms.Select(r => new
            {
                RoomId = r.Key,
                RoomCode = r.Value.RoomCode,
                PlayerCount = r.Value.Players.Count,
                MaxPlayers = r.Value.MaxPlayers,
                IsGameStarted = r.Value.IsGameStarted
            }).ToList();

            return Ok(new
            {
                OnlinePlayerCCU = RoomManager.GetCCU(),
                ActiveRooms = RoomManager.ActiveRooms.Count,
                ActiveRoomsDetail = roomDetails,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint: GET /api/admin/transactions
        /// Lấy danh sách giao dịch PayOS VietQR & Nạp vật phẩm thực tế từ Database.
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int limit = 50)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Include(t => t.User)
                    .Include(t => t.ShopItem)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(limit)
                    .Select(t => new AdminTransactionDto
                    {
                        TransactionId = t.TransactionId,
                        OrderCode = t.OrderCode,
                        UserId = t.UserId,
                        Username = t.User != null ? t.User.Username : ("User #" + t.UserId),
                        ShopItemId = t.ShopItemId,
                        ItemName = t.ShopItem != null ? t.ShopItem.Name : (t.TransactionType ?? "Vật phẩm"),
                        TransactionType = t.TransactionType ?? "Purchase",
                        Amount = t.Amount,
                        CurrencyType = t.CurrencyType ?? "VND",
                        PaymentMethod = t.PaymentMethod ?? "PayOS VietQR",
                        Status = t.Status ?? "PENDING",
                        CreatedAt = t.CreatedAt,
                        PaidAt = t.PaidAt
                    })
                    .ToListAsync();

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy danh sách giao dịch.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/admin/analytics/payment
        /// Thống kê doanh thu tiền thật (VND) và chỉ số kinh tế Game (Gem/Coins) thực tế.
        /// </summary>
        [HttpGet("analytics/payment")]
        public async Task<IActionResult> GetPaymentAnalytics()
        {
            try
            {
                var totalTransactions = await _context.Transactions.CountAsync();
                var successfulTransactions = await _context.Transactions.CountAsync(t => t.Status == "PAID" || t.Status == "SUCCESS");
                var pendingTransactions = await _context.Transactions.CountAsync(t => t.Status == "PENDING");
                var cancelledTransactions = await _context.Transactions.CountAsync(t => t.Status == "CANCELLED");

                var totalRevenueVND = await _context.Transactions
                    .Where(t => (t.Status == "PAID" || t.Status == "SUCCESS") && (t.CurrencyType == null || t.CurrencyType == "VND" || t.CurrencyType == "vnd"))
                    .SumAsync(t => (long)t.Amount);

                var totalGems = await _context.PlayerProfiles.SumAsync(p => (long)p.PremiumCurrency);
                var totalCoins = await _context.PlayerProfiles.SumAsync(p => (long)p.StandardCurrency);

                // Doanh thu theo từng ngày trong 7 ngày gần nhất
                var now = DateTime.UtcNow;
                var sevenDaysAgo = now.Date.AddDays(-6);
                var recentTxs = await _context.Transactions
                    .Where(t => t.CreatedAt >= sevenDaysAgo && (t.Status == "PAID" || t.Status == "SUCCESS"))
                    .ToListAsync();

                var dailyRevenue = new List<DailyRevenueDto>();
                for (int i = 6; i >= 0; i--)
                {
                    var targetDate = now.Date.AddDays(-i);
                    var dateStr = targetDate.ToString("yyyy-MM-dd");
                    var dayLabel = targetDate.ToString("ddd");
                    var dayTxs = recentTxs.Where(t => t.CreatedAt.Date == targetDate).ToList();

                    dailyRevenue.Add(new DailyRevenueDto
                    {
                        Date = dateStr,
                        DayLabel = dayLabel,
                        Revenue = dayTxs.Sum(t => (long)t.Amount),
                        TransactionCount = dayTxs.Count
                    });
                }

                // Doanh thu theo tuần cho 5 tuần gần nhất
                var weeklyRevenue = new List<WeeklyRevenueDto>();
                for (int w = 4; w >= 0; w--)
                {
                    var weekStart = now.Date.AddDays(-(w * 7 + 6));
                    var weekEnd = now.Date.AddDays(-(w * 7));
                    var weekTxs = await _context.Transactions
                        .Where(t => t.CreatedAt.Date >= weekStart && t.CreatedAt.Date <= weekEnd && (t.Status == "PAID" || t.Status == "SUCCESS"))
                        .ToListAsync();

                    weeklyRevenue.Add(new WeeklyRevenueDto
                    {
                        WeekLabel = w == 0 ? "Wk CUR" : $"Wk -{w}",
                        Revenue = weekTxs.Sum(t => (long)t.Amount),
                        GemVolume = weekTxs.Count * 100
                    });
                }

                return Ok(new PaymentAnalyticsResponse
                {
                    TotalRevenueVND = totalRevenueVND,
                    TotalTransactions = totalTransactions,
                    SuccessfulTransactions = successfulTransactions,
                    PendingTransactions = pendingTransactions,
                    CancelledTransactions = cancelledTransactions,
                    TotalGemsInEconomy = totalGems,
                    TotalCoinsInEconomy = totalCoins,
                    DailyRevenue = dailyRevenue,
                    WeeklyRevenue = weeklyRevenue
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy thống kê thanh toán.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/admin/analytics/players
        /// Thống kê lượt chơi và người chơi thực tế theo từng ngày trong tuần từ bảng RunHistories.
        /// </summary>
        [HttpGet("analytics/players")]
        public async Task<IActionResult> GetPlayerAnalytics()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalRuns = await _context.RunHistories.CountAsync();
                var onlineCCU = RoomManager.GetCCU();
                var activeRooms = RoomManager.ActiveRooms.Count;

                var now = DateTime.UtcNow;
                var sevenDaysAgo = now.Date.AddDays(-6);
                var recentRuns = await _context.RunHistories
                    .Where(r => r.PlayedAt >= sevenDaysAgo)
                    .ToListAsync();

                var dailyActivity = new List<DailyPlayerActivityDto>();
                for (int i = 6; i >= 0; i--)
                {
                    var targetDate = now.Date.AddDays(-i);
                    var dateStr = targetDate.ToString("yyyy-MM-dd");
                    var dayLabel = targetDate.ToString("ddd");
                    var dayRuns = recentRuns.Where(r => r.PlayedAt.Date == targetDate).ToList();

                    dailyActivity.Add(new DailyPlayerActivityDto
                    {
                        DayName = dayLabel,
                        Date = dateStr,
                        TotalRuns = dayRuns.Count,
                        SurvivedRuns = dayRuns.Count(r => r.WavesSurvived > 0),
                        DefeatRuns = dayRuns.Count(r => r.WavesSurvived == 0),
                        UniquePlayers = dayRuns.Select(r => r.UserId).Distinct().Count()
                    });
                }

                return Ok(new PlayerAnalyticsResponse
                {
                    TotalRegisteredUsers = totalUsers,
                    TotalRunsPlayed = totalRuns,
                    OnlineCCU = onlineCCU,
                    ActiveRooms = activeRooms,
                    DailyActivity = dailyActivity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy thống kê người chơi.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint: GET /api/admin/recent-activities
        /// Cung cấp luồng sự kiện nhật ký hệ thống thực tế (Live Hangar Logs):
        /// - Các lượt chơi vừa kết thúc (RunHistories)
        /// - Người chơi mới đăng ký (Users)
        /// - Giao dịch thanh toán PayOS (Transactions)
        /// - Trạng thái phòng Co-op đang chơi
        /// </summary>
        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 25)
        {
            try
            {
                var activities = new List<RecentActivityDto>();

                // 1. Lượt chơi gần nhất
                var latestRuns = await _context.RunHistories
                    .Include(r => r.User)
                    .Include(r => r.Character)
                    .OrderByDescending(r => r.PlayedAt)
                    .Take(10)
                    .ToListAsync();

                foreach (var r in latestRuns)
                {
                    string userName = r.User != null ? r.User.Username : $"Pilot-{r.UserId}";
                    string charName = r.Character != null ? r.Character.Name : "Chiến binh";
                    activities.Add(new RecentActivityDto
                    {
                        Timestamp = r.PlayedAt,
                        TimeFormatted = r.PlayedAt.ToString("HH:mm:ss"),
                        ActivityType = "Run",
                        Description = $"{userName} ({charName}) kết thúc thám hiểm: Vượt {r.WavesSurvived} wave, hạ {r.EnemiesKilled} quái ({r.DurationSeconds}s).",
                        Severity = r.WavesSurvived > 0 ? "success" : "warning"
                    });
                }

                // 2. Tài khoản mới đăng ký
                var latestUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                foreach (var u in latestUsers)
                {
                    activities.Add(new RecentActivityDto
                    {
                        Timestamp = u.CreatedAt,
                        TimeFormatted = u.CreatedAt.ToString("HH:mm:ss"),
                        ActivityType = "Crew",
                        Description = $"Phi hành gia mới '{u.Username}' vừa gia nhập căn cứ Rogue-Kie.",
                        Severity = "info"
                    });
                }

                // 3. Giao dịch thanh toán gần nhất
                var latestTxs = await _context.Transactions
                    .Include(t => t.User)
                    .Include(t => t.ShopItem)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                foreach (var t in latestTxs)
                {
                    string uName = t.User != null ? t.User.Username : $"User-{t.UserId}";
                    string iName = t.ShopItem != null ? t.ShopItem.Name : (t.TransactionType ?? "Vật phẩm");
                    string statusTxt = t.Status == "PAID" ? "Thành công" : (t.Status == "PENDING" ? "Chờ quét mã" : "Đã hủy");
                    string sev = t.Status == "PAID" ? "success" : (t.Status == "PENDING" ? "info" : "danger");

                    activities.Add(new RecentActivityDto
                    {
                        Timestamp = t.PaidAt ?? t.CreatedAt,
                        TimeFormatted = (t.PaidAt ?? t.CreatedAt).ToString("HH:mm:ss"),
                        ActivityType = "Payment",
                        Description = $"Giao dịch #{t.OrderCode} ({uName}): {t.Amount:N0} VND - {iName} [{statusTxt}].",
                        Severity = sev
                    });
                }

                // 4. Phòng Co-op đang mở
                if (RoomManager.ActiveRooms.Count > 0)
                {
                    foreach (var kvp in RoomManager.ActiveRooms.Take(3))
                    {
                        activities.Add(new RecentActivityDto
                        {
                            Timestamp = DateTime.UtcNow,
                            TimeFormatted = DateTime.UtcNow.ToString("HH:mm:ss"),
                            ActivityType = "Co-op",
                            Description = $"Phòng Co-op #{kvp.Key} (Mã: {kvp.Value.RoomCode}) đang hoạt động với {kvp.Value.Players.Count} người.",
                            Severity = "info"
                        });
                    }
                }

                var sorted = activities.OrderByDescending(a => a.Timestamp).Take(limit).ToList();
                return Ok(sorted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi khi lấy nhật ký hoạt động gần đây.", Detail = ex.Message });
            }
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

                // Phát tín hiệu thời gian thực qua SignalR để đá người chơi ngay lập tức (tiếng Anh)
                string banReasonEn = "Your account has been suspended by an Administrator. You have been disconnected.";
                await _hubContext.Clients.All.SendAsync("OnUserBanned", user.Username, banReasonEn);

                // Dọn dẹp phòng Co-op nếu user này đang tham gia
                foreach (var kvp in RoomManager.ActiveRooms)
                {
                    var room = kvp.Value;
                    var playerInRoom = room.Players.FirstOrDefault(p => string.Equals(p.Username, user.Username, StringComparison.OrdinalIgnoreCase));
                    if (playerInRoom != null)
                    {
                        room.Players.Remove(playerInRoom);
                        RoomManager.ConnectionToRoom.TryRemove(playerInRoom.ConnectionId, out _);
                        await _hubContext.Clients.Group(room.RoomCode).SendAsync("OnPlayerDisconnected", playerInRoom.Username, playerInRoom.ConnectionId);
                    }
                }

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
