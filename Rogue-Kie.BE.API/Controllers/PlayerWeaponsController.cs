using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller quản lý kho vũ khí của người chơi (Player Weapons Inventory).
    /// Hỗ trợ truy vấn danh sách vũ khí đã mở khóa vĩnh viễn (Unlocked) và kích hoạt mở khóa sau khi mua.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerWeaponsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlayerWeaponsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Endpoint: GET /api/playerweapons/my-weapons
        /// Lấy toàn bộ danh sách vũ khí mà người chơi hiện tại đã mở khóa (IsUnlocked = true).
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

            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return Ok(Array.Empty<object>());
            }

            // Đồng bộ tự động mở khóa các vũ khí từ giao dịch đã thanh toán thành công trong quá khứ (Retroactive Unlock)
            await SyncPaidWeaponsAsync(profile.ProfileId, userId);

            var unlockedWeapons = await _context.PlayerWeapons
                .Include(pw => pw.WeaponConfig)
                .Where(pw => pw.ProfileId == profile.ProfileId && pw.IsUnlocked)
                .Select(pw => new
                {
                    pw.Id,
                    pw.ProfileId,
                    pw.WeaponConfigId,
                    WeaponName = pw.WeaponConfig != null ? pw.WeaponConfig.WeaponName : "",
                    PrefabName = pw.WeaponConfig != null ? pw.WeaponConfig.PrefabName : "",
                    WeaponType = pw.WeaponConfig != null ? pw.WeaponConfig.WeaponType : "Rifle",
                    Rarity = pw.WeaponConfig != null ? pw.WeaponConfig.Rarity : "Common",
                    pw.IsUnlocked,
                    pw.UnlockedAt
                })
                .ToListAsync();

            return Ok(unlockedWeapons);
        }

        /// <summary>
        /// Endpoint: POST /api/playerweapons/unlock/{weaponConfigId}
        /// Mở khóa trực tiếp một khẩu súng cho người chơi hiện tại theo WeaponConfigId (hỗ trợ tích hợp hoặc dev test).
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

            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new PlayerProfile
                {
                    UserId = userId,
                    DisplayName = "Player " + userId,
                    StandardCurrency = 0,
                    PremiumCurrency = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PlayerProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            var weaponConfig = await _context.WeaponConfigs.FindAsync(weaponConfigId);
            if (weaponConfig == null)
            {
                return NotFound("Không tìm thấy cấu hình vũ khí với ID tương ứng.");
            }

            var playerWeapon = await _context.PlayerWeapons
                .FirstOrDefaultAsync(pw => pw.ProfileId == profile.ProfileId && pw.WeaponConfigId == weaponConfigId);

            if (playerWeapon == null)
            {
                playerWeapon = new PlayerWeapon
                {
                    ProfileId = profile.ProfileId,
                    WeaponConfigId = weaponConfigId,
                    IsUnlocked = true,
                    UnlockedAt = DateTime.UtcNow
                };
                _context.PlayerWeapons.Add(playerWeapon);
            }
            else
            {
                playerWeapon.IsUnlocked = true;
                playerWeapon.UnlockedAt ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Đã mở khóa vĩnh viễn vũ khí '{weaponConfig.WeaponName}' thành công!",
                weaponConfigId,
                weaponName = weaponConfig.WeaponName,
                prefabName = weaponConfig.PrefabName
            });
        }

        /// <summary>
        /// Endpoint: POST /api/playerweapons/dev-reset-my-weapons
        /// Xóa sạch dữ liệu súng đã mở khóa và các giao dịch nạp tiền của người chơi hiện tại (phục vụ Testing).
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

            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                var pws = await _context.PlayerWeapons.Where(pw => pw.ProfileId == profile.ProfileId).ToListAsync();
                _context.PlayerWeapons.RemoveRange(pws);
            }

            var txs = await _context.Transactions.Where(t => t.UserId == userId).ToListAsync();
            _context.Transactions.RemoveRange(txs);

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa sạch dữ liệu Kho Vũ Khí và Giao Dịch test của bạn trên DB!" });
        }

        private async Task SyncPaidWeaponsAsync(int profileId, int userId)
        {
            try
            {
                var paidTxs = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Status == "PAID")
                    .ToListAsync();

                foreach (var tx in paidTxs)
                {
                    string desc = (tx.ReferenceCode ?? "").ToLower();
                    int? shopId = tx.ShopItemId;
                    int amount = tx.Amount;

                    WeaponConfig? weapon = null;
                    if (shopId.HasValue)
                    {
                        var shopItem = await _context.ShopItems.FindAsync(shopId.Value);
                        if (shopItem != null)
                        {
                            weapon = await _context.WeaponConfigs
                                .FirstOrDefaultAsync(w => w.WeaponName.ToLower() == shopItem.Name.ToLower() 
                                                       || w.PrefabName.ToLower().Contains(shopItem.Name.ToLower()));
                        }
                    }

                    if (weapon == null)
                    {
                        if (desc.Contains("missile"))
                            weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("Missile"));
                        else if (desc.Contains("rocket") || desc.Contains("bazooka"))
                            weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("Rocket"));
                        else if (desc.Contains("ak") || desc.Contains("gold") || amount == 2000)
                            weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("AK_47A_Gold") || w.WeaponName.Contains("AK"));
                    }

                    // Nếu WeaponConfig chưa có trong DB, tự động tạo mới
                    if (weapon == null)
                    {
                        if (desc.Contains("missile"))
                        {
                            weapon = new WeaponConfig
                            {
                                WeaponName = "Missile Launcher",
                                PrefabName = "Missile_Launcher",
                                WeaponType = "Launcher",
                                Rarity = "Epic",
                                FireRate = 1.2f,
                                BulletsPerShot = 1,
                                SpreadAngle = 0f,
                                RecoilDistance = 0.4f,
                                BulletId = 3
                            };
                            _context.WeaponConfigs.Add(weapon);
                            await _context.SaveChangesAsync();
                        }
                        else if (desc.Contains("rocket") || desc.Contains("bazooka"))
                        {
                            weapon = new WeaponConfig
                            {
                                WeaponName = "Rocket Launcher",
                                PrefabName = "Rocket_Launcher",
                                WeaponType = "Launcher",
                                Rarity = "Epic",
                                FireRate = 1.5f,
                                BulletsPerShot = 1,
                                SpreadAngle = 0f,
                                RecoilDistance = 0.5f,
                                BulletId = 3
                            };
                            _context.WeaponConfigs.Add(weapon);
                            await _context.SaveChangesAsync();
                        }
                        else if (desc.Contains("ak") || desc.Contains("gold") || amount == 2000)
                        {
                            weapon = new WeaponConfig
                            {
                                WeaponName = "AK-47 Gold",
                                PrefabName = "AK_47A_Gold",
                                WeaponType = "Rifle",
                                Rarity = "Legendary",
                                FireRate = 0.15f,
                                BulletsPerShot = 1,
                                SpreadAngle = 3.0f,
                                RecoilDistance = 0.15f,
                                BulletId = 4
                            };
                            _context.WeaponConfigs.Add(weapon);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (weapon != null)
                    {
                        var pw = await _context.PlayerWeapons
                            .FirstOrDefaultAsync(p => p.ProfileId == profileId && p.WeaponConfigId == weapon.Id);

                        if (pw == null)
                        {
                            pw = new PlayerWeapon
                            {
                                ProfileId = profileId,
                                WeaponConfigId = weapon.Id,
                                IsUnlocked = true,
                                UnlockedAt = tx.PaidAt ?? DateTime.UtcNow
                            };
                            _context.PlayerWeapons.Add(pw);
                            await _context.SaveChangesAsync();
                        }
                        else if (!pw.IsUnlocked)
                        {
                            pw.IsUnlocked = true;
                            pw.UnlockedAt ??= tx.PaidAt ?? DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SyncPaidWeaponsAsync Error] {ex.Message}");
            }
        }
    }
}
