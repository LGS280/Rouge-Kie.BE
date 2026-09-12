using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public class ShopItemService : IShopItemService
    {
        private readonly AppDbContext _context;

        public ShopItemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShopItemResponse>> GetAllAsync()
        {
            return await _context.ShopItems
                .Select(s => new ShopItemResponse
                {
                    ShopItemId = s.ShopItemId,
                    Name = s.Name,
                    ItemType = s.ItemType,
                    Description = s.Description,
                    Price = s.Price,
                    CurrencyType = s.CurrencyType
                })
                .ToListAsync();
        }

        public async Task<ShopItemResponse?> GetByIdAsync(int id)
        {
            var s = await _context.ShopItems.FindAsync(id);
            if (s == null) return null;

            return new ShopItemResponse
            {
                ShopItemId = s.ShopItemId,
                Name = s.Name,
                ItemType = s.ItemType,
                Description = s.Description,
                Price = s.Price,
                CurrencyType = s.CurrencyType
            };
        }

        public async Task<ShopItemResponse> CreateAsync(CreateShopItemRequest request)
        {
            var entity = new ShopItem
            {
                Name = request.Name,
                ItemType = request.ItemType,
                Description = request.Description,
                Price = request.Price,
                CurrencyType = request.CurrencyType
            };

            _context.ShopItems.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.ShopItemId) ?? throw new System.Exception("Failed to retrieve created shop item");
        }

        public async Task<ShopItemResponse?> UpdateAsync(int id, UpdateShopItemRequest request)
        {
            var entity = await _context.ShopItems.FindAsync(id);
            if (entity == null) return null;

            entity.Name = request.Name;
            entity.ItemType = request.ItemType;
            entity.Description = request.Description;
            entity.Price = request.Price;
            entity.CurrencyType = request.CurrencyType;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.ShopItemId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ShopItems.FindAsync(id);
            if (entity == null) return false;

            _context.ShopItems.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BuyItemResponse> BuyItemAsync(int userId, int shopItemId)
        {
            var shopItem = await _context.ShopItems.FindAsync(shopItemId);
            if (shopItem == null)
            {
                return new BuyItemResponse
                {
                    Success = false,
                    Message = "Không tìm thấy vật phẩm trong Cửa Hàng."
                };
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
                    UpdatedAt = System.DateTime.UtcNow
                };
                _context.PlayerProfiles.Add(profile);
            }

            string currency = (shopItem.CurrencyType ?? "GEMS").ToUpper();
            bool isGem = currency == "GEMS" || currency == "PREMIUM" || currency == "GEM";

            if (isGem)
            {
                if (profile.PremiumCurrency < shopItem.Price)
                {
                    return new BuyItemResponse
                    {
                        Success = false,
                        Message = $"Bạn không đủ Gem để mua vật phẩm này (Cần {shopItem.Price} Gem, bạn có {profile.PremiumCurrency} Gem).",
                        RemainingStandardCurrency = profile.StandardCurrency,
                        RemainingPremiumCurrency = profile.PremiumCurrency
                    };
                }
                profile.PremiumCurrency -= shopItem.Price;
            }
            else
            {
                if (profile.StandardCurrency < shopItem.Price)
                {
                    return new BuyItemResponse
                    {
                        Success = false,
                        Message = $"Bạn không đủ Vàng để mua vật phẩm này (Cần {shopItem.Price} Vàng, bạn có {profile.StandardCurrency} Vàng).",
                        RemainingStandardCurrency = profile.StandardCurrency,
                        RemainingPremiumCurrency = profile.PremiumCurrency
                    };
                }
                profile.StandardCurrency -= shopItem.Price;
            }

            // Nếu mua gói đổi Vàng bằng Gem
            if (shopItem.ItemType == "EXCHANGE_COINS" || shopItem.ItemType == "COINS")
            {
                int coinsToAdd = shopItem.Price * 100; // 1 Gem = 100 Coins
                profile.StandardCurrency += coinsToAdd;
            }

            // Tự động lưu mở khóa vào PlayerWeapons nếu vật phẩm là vũ khí
            await AutoUnlockWeaponIfApplicableAsync(profile.ProfileId, shopItem);

            profile.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new BuyItemResponse
            {
                Success = true,
                Message = $"Mua thành công {shopItem.Name}!",
                ShopItemId = shopItem.ShopItemId,
                ItemName = shopItem.Name,
                RemainingStandardCurrency = profile.StandardCurrency,
                RemainingPremiumCurrency = profile.PremiumCurrency,
                PurchasedAt = System.DateTime.UtcNow
            };
        }

        private async Task AutoUnlockWeaponIfApplicableAsync(int profileId, ShopItem shopItem)
        {
            string itemName = shopItem.Name ?? "";
            string lowerName = itemName.ToLower();

            // Tìm WeaponConfig tương ứng trong Database
            var weapon = await _context.WeaponConfigs
                .FirstOrDefaultAsync(w => w.WeaponName.ToLower() == lowerName 
                                       || w.PrefabName.ToLower().Contains(lowerName) 
                                       || lowerName.Contains(w.WeaponName.ToLower()));

            // Fallback theo từ khóa đặc trưng của súng
            if (weapon == null)
            {
                if (lowerName.Contains("ak") || lowerName.Contains("gold"))
                    weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("AK_47A_Gold") || w.WeaponName.Contains("AK"));
                else if (lowerName.Contains("missile"))
                    weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("Missile"));
                else if (lowerName.Contains("rocket") || lowerName.Contains("bazooka"))
                    weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("Rocket"));
            }

            // Nếu WeaponConfig chưa có trong DB, tự động tạo mới
            if (weapon == null)
            {
                if (lowerName.Contains("ak") || lowerName.Contains("gold"))
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
                else if (lowerName.Contains("missile"))
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
                else if (lowerName.Contains("rocket") || lowerName.Contains("bazooka"))
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
            }

            if (weapon != null)
            {
                var playerWeapon = await _context.PlayerWeapons
                    .FirstOrDefaultAsync(pw => pw.ProfileId == profileId && pw.WeaponConfigId == weapon.Id);

                if (playerWeapon == null)
                {
                    playerWeapon = new PlayerWeapon
                    {
                        ProfileId = profileId,
                        WeaponConfigId = weapon.Id,
                        IsUnlocked = true,
                        UnlockedAt = System.DateTime.UtcNow
                    };
                    _context.PlayerWeapons.Add(playerWeapon);
                }
                else
                {
                    playerWeapon.IsUnlocked = true;
                    playerWeapon.UnlockedAt ??= System.DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
