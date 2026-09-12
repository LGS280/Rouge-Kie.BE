using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rogue_Kie.BE.Contracts.Payment;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.Payment
{
    public class PayOSService : IPayOSService
    {
        private readonly AppDbContext _context;
        private readonly PayOSSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(
            AppDbContext context,
            IOptions<PayOSSettings> settings,
            HttpClient httpClient,
            ILogger<PayOSService> logger)
        {
            _context = context;
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePaymentLinkAsync(int userId, CreatePaymentLinkRequest request)
        {
            // 1. Tạo orderCode ngẫu nhiên dạng số nguyên dương (100000 -> 999999999)
            long orderCode = DateTime.UtcNow.Ticks % 100000000;
            if (orderCode < 100000) orderCode += 100000;

            int amount = request.Amount;
            if (amount <= 0 && request.ShopItemId.HasValue)
            {
                var shopItem = await _context.ShopItems.FindAsync(request.ShopItemId.Value);
                if (shopItem != null)
                {
                    amount = shopItem.Price;
                    if (string.IsNullOrEmpty(request.Description))
                    {
                        request.Description = shopItem.Name;
                    }
                }
            }

            if (amount <= 0) amount = 10000; // Mặc định 10,000 VND

            string description = "Nap Gem RogueKie";
            if (!string.IsNullOrEmpty(request.Description))
            {
                // Xóa ký tự tiếng Việt có dấu và ký tự đặc biệt theo yêu cầu của PayOS (tối đa 25 ký tự)
                string unaccented = RemoveAccents(request.Description);
                description = System.Text.RegularExpressions.Regex.Replace(unaccented, @"[^a-zA-Z0-9 ]", "");
            }
            if (string.IsNullOrWhiteSpace(description)) description = "Nap Gem RogueKie";
            if (description.Length > 25) description = description.Substring(0, 25);

            // Kiểm tra shopItemId có tồn tại trong bảng ShopItems không trước khi gán để tránh lỗi Foreign Key
            int? validShopItemId = null;
            if (request.ShopItemId.HasValue && request.ShopItemId.Value > 0)
            {
                if (await _context.ShopItems.AnyAsync(s => s.ShopItemId == request.ShopItemId.Value))
                {
                    validShopItemId = request.ShopItemId.Value;
                }
            }

            string savedRef = !string.IsNullOrEmpty(request.Description) ? request.Description : orderCode.ToString();
            if (savedRef.Length > 100) savedRef = savedRef.Substring(0, 100);

            // 2. Tạo Transaction PENDING trong Database
            var transaction = new Transaction
            {
                UserId = userId,
                ShopItemId = validShopItemId,
                OrderCode = orderCode,
                TransactionType = "PAYMENT",
                Amount = amount,
                CurrencyType = string.IsNullOrEmpty(request.CurrencyType) ? "GEMS" : request.CurrencyType,
                PaymentMethod = "PAYOS",
                Status = "PENDING",
                ReferenceCode = savedRef,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // 3. Gọi API PayOS sinh Checkout Link & VietQR Code
            string returnUrl = string.IsNullOrEmpty(_settings.ReturnUrl) ? "https://roguekie.com/payment/success" : _settings.ReturnUrl;
            string cancelUrl = string.IsNullOrEmpty(_settings.CancelUrl) ? "https://roguekie.com/payment/cancel" : _settings.CancelUrl;

            string clientId = string.IsNullOrEmpty(_settings.ClientId) ? "e9157d63-27cc-4afe-bf60-b4c48dad5786" : _settings.ClientId;
            string apiKey = string.IsNullOrEmpty(_settings.ApiKey) ? "091a6e8e-a064-4ada-ac74-8c571db289e3" : _settings.ApiKey;
            string checksumKey = string.IsNullOrEmpty(_settings.ChecksumKey) ? "b3923181eb25e58c09b5835e3ed7ac4a6a427c602b5348d463177f1b9abb3463" : _settings.ChecksumKey;

            // Tính chữ ký Signature cho PayOS
            // Format chuẩn: amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}
            string signatureData = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            string signature = ComputeHmacSha256(signatureData, checksumKey);

            var payosPayload = new
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                signature = signature
            };

            string checkoutUrl = $"https://pay.payos.vn/web/{orderCode}";
            string qrCodeUrl = $"https://img.vietqr.io/image/970422-0344536487-compact2.jpg?amount={amount}&addInfo={Uri.EscapeDataString(description)}&accountName=TRAN%20VU%20QUOC%20DAI";

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests");
                requestMessage.Headers.Add("x-client-id", clientId);
                requestMessage.Headers.Add("x-api-key", apiKey);
                requestMessage.Headers.Add("User-Agent", "RogueKie-Backend");
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(payosPayload), Encoding.UTF8, "application/json");

                var responseMessage = await _httpClient.SendAsync(requestMessage);
                string responseJson = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation($"[PayOSService] Response tu PayOS API ({responseMessage.StatusCode}): {responseJson}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;
                    string codeStr = GetJsonCode(root);

                    if ((codeStr == "00" || codeStr == "0") && root.TryGetProperty("data", out var dataProp))
                    {
                        if (dataProp.TryGetProperty("checkoutUrl", out var urlProp) && urlProp.ValueKind == JsonValueKind.String)
                        {
                            checkoutUrl = urlProp.GetString() ?? checkoutUrl;
                        }

                        string bin = "970422";
                        if (dataProp.TryGetProperty("bin", out var binProp) && binProp.ValueKind == JsonValueKind.String)
                        {
                            bin = binProp.GetString() ?? "970422";
                        }

                        string accountNumber = "";
                        if (dataProp.TryGetProperty("accountNumber", out var accProp) && accProp.ValueKind == JsonValueKind.String)
                        {
                            accountNumber = accProp.GetString() ?? "";
                        }

                        string accountName = "TRAN VU QUOC DAI";
                        if (dataProp.TryGetProperty("accountName", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                        {
                            accountName = nameProp.GetString() ?? "TRAN VU QUOC DAI";
                        }

                        if (dataProp.TryGetProperty("qrCode", out var qrProp) && qrProp.ValueKind == JsonValueKind.String)
                        {
                            string rawQr = qrProp.GetString() ?? "";
                            if (rawQr.StartsWith("http"))
                            {
                                qrCodeUrl = rawQr;
                            }
                            else if (!string.IsNullOrEmpty(rawQr))
                            {
                                qrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=400x400&data={Uri.EscapeDataString(rawQr)}";
                            }
                            else if (!string.IsNullOrEmpty(accountNumber))
                            {
                                qrCodeUrl = $"https://img.vietqr.io/image/{bin}-{accountNumber}-compact2.jpg?amount={amount}&addInfo={Uri.EscapeDataString(description)}&accountName={Uri.EscapeDataString(accountName)}";
                            }
                        }
                        else if (!string.IsNullOrEmpty(accountNumber))
                        {
                            qrCodeUrl = $"https://img.vietqr.io/image/{bin}-{accountNumber}-compact2.jpg?amount={amount}&addInfo={Uri.EscapeDataString(description)}&accountName={Uri.EscapeDataString(accountName)}";
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[PayOSService] PayOS API tra ve code khac 00: {codeStr} - JSON: {responseJson}");
                    }
                }
                else
                {
                    _logger.LogError($"[PayOSService] PayOS API tra ve loi ({responseMessage.StatusCode}): {responseJson}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PayOSService] Khong the ket noi PayOS API live. Dung URL khoi tao mac dinh.");
            }

            // Cập nhật URL vào Transaction
            transaction.PaymentUrl = checkoutUrl;
            transaction.QrCodeUrl = qrCodeUrl;
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                TransactionId = transaction.TransactionId,
                OrderCode = transaction.OrderCode,
                Amount = transaction.Amount,
                Description = description,
                PaymentUrl = checkoutUrl,
                QrCodeUrl = qrCodeUrl,
                Status = transaction.Status,
                CurrencyType = transaction.CurrencyType ?? "VND",
                CreatedAt = transaction.CreatedAt,
                PaidAt = transaction.PaidAt
            };
        }

        public async Task<bool> ProcessWebhookAsync(PayOSWebhookRequest webhook)
        {
            if (webhook == null || webhook.Data == null) return false;

            long orderCode = webhook.Data.OrderCode;
            _logger.LogInformation($"[PayOSService] Nhan Webhook tu PayOS cho OrderCode: {orderCode}, Code: {webhook.Code}");

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode || t.ReferenceCode == orderCode.ToString());

            if (transaction == null)
            {
                _logger.LogWarning($"[PayOSService] Khong tim thay Transaction voi OrderCode: {orderCode}");
                return false;
            }

            if (transaction.Status == "PAID")
            {
                _logger.LogInformation($"[PayOSService] OrderCode {orderCode} da duoc thanh toan truoc do (Idempotent).");
                return true;
            }

            // Đổi trạng thái giao dịch sang PAID
            transaction.Status = "PAID";
            transaction.PaidAt = DateTime.UtcNow;

            // Nạp tiền / Gem / Coin cho PlayerProfile của người chơi
            await FulfillUserRewardAsync(transaction);

            await _context.SaveChangesAsync();
            _logger.LogInformation($"[PayOSService] Thanh toan THANH CONG cho OrderCode: {orderCode}, User: {transaction.UserId}!");
            return true;
        }

        public async Task<PaymentResponse?> GetPaymentStatusAsync(long orderCode)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode || t.ReferenceCode == orderCode.ToString());

            if (transaction == null) return null;

            // Nếu trạng thái trong DB đang là PENDING, tự động gọi PayOS API để sync tức thì (Đặc biệt hữu ích khi chạy Localhost không có Webhook Public URL)
            if (transaction.Status == "PENDING")
            {
                try
                {
                    string clientId = string.IsNullOrEmpty(_settings.ClientId) ? "e9157d63-27cc-4afe-bf60-b4c48dad5786" : _settings.ClientId;
                    string apiKey = string.IsNullOrEmpty(_settings.ApiKey) ? "091a6e8e-a064-4ada-ac74-8c571db289e3" : _settings.ApiKey;

                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}");
                    requestMessage.Headers.Add("x-client-id", clientId);
                    requestMessage.Headers.Add("x-api-key", apiKey);
                    requestMessage.Headers.Add("User-Agent", "RogueKie-Backend");

                    var responseMessage = await _httpClient.SendAsync(requestMessage);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string responseJson = await responseMessage.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseJson);
                        var root = doc.RootElement;
                        string codeStr = GetJsonCode(root);

                        if ((codeStr == "00" || codeStr == "0") && root.TryGetProperty("data", out var dataProp))
                        {
                            if (dataProp.TryGetProperty("status", out var statusProp) && statusProp.ValueKind == JsonValueKind.String)
                            {
                                string payosStatus = statusProp.GetString() ?? "";
                                if (payosStatus == "PAID")
                                {
                                    transaction.Status = "PAID";
                                    transaction.PaidAt = DateTime.UtcNow;
                                    await FulfillUserRewardAsync(transaction);
                                    await _context.SaveChangesAsync();
                                    _logger.LogInformation($"[PayOSService] Sync trang thai THANH CONG tu PayOS API cho OrderCode: {orderCode}!");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"[PayOSService] Khong the sync trang thai tu PayOS API cho OrderCode {orderCode}");
                }
            }

            return new PaymentResponse
            {
                TransactionId = transaction.TransactionId,
                OrderCode = transaction.OrderCode,
                Amount = transaction.Amount,
                Description = transaction.ReferenceCode ?? "Nap Gem RogueKie",
                PaymentUrl = transaction.PaymentUrl ?? "",
                QrCodeUrl = transaction.QrCodeUrl ?? "",
                Status = transaction.Status ?? "PENDING",
                CurrencyType = transaction.CurrencyType ?? "VND",
                CreatedAt = transaction.CreatedAt,
                PaidAt = transaction.PaidAt
            };
        }

        public async Task<bool> CancelPaymentAsync(long orderCode)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode || t.ReferenceCode == orderCode.ToString());

            if (transaction == null || transaction.Status == "PAID") return false;

            // Gọi PayOS API cancel đơn hàng trên Server PayOS
            try
            {
                string clientId = string.IsNullOrEmpty(_settings.ClientId) ? "e9157d63-27cc-4afe-bf60-b4c48dad5786" : _settings.ClientId;
                string apiKey = string.IsNullOrEmpty(_settings.ApiKey) ? "091a6e8e-a064-4ada-ac74-8c571db289e3" : _settings.ApiKey;

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}/cancel");
                requestMessage.Headers.Add("x-client-id", clientId);
                requestMessage.Headers.Add("x-api-key", apiKey);
                requestMessage.Headers.Add("User-Agent", "RogueKie-Backend");
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(new { cancellationReason = "Huy don hang" }), Encoding.UTF8, "application/json");

                var responseMessage = await _httpClient.SendAsync(requestMessage);
                string responseJson = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation($"[PayOSService] Cancel PayOS Response ({responseMessage.StatusCode}): {responseJson}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"[PayOSService] Khong the ket noi PayOS API cancel cho OrderCode {orderCode}");
            }

            transaction.Status = "CANCELLED";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CancelAllPendingPaymentsAsync(int userId)
        {
            var pendingTransactions = await _context.Transactions
                .Where(t => (userId <= 0 || t.UserId == userId) && t.Status == "PENDING")
                .ToListAsync();

            int count = 0;
            foreach (var t in pendingTransactions)
            {
                bool success = await CancelPaymentAsync(t.OrderCode);
                if (success) count++;
            }

            return count;
        }

        public async Task<bool> SimulateSuccessAsync(long orderCode)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode || t.ReferenceCode == orderCode.ToString());

            if (transaction == null) return false;

            if (transaction.Status == "PAID") return true;

            transaction.Status = "PAID";
            transaction.PaidAt = DateTime.UtcNow;

            await FulfillUserRewardAsync(transaction);

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task FulfillUserRewardAsync(Transaction transaction)
        {
            var profile = await _context.PlayerProfiles
                .FirstOrDefaultAsync(p => p.UserId == transaction.UserId);

            if (profile == null)
            {
                profile = new PlayerProfile
                {
                    UserId = transaction.UserId,
                    DisplayName = "Player " + transaction.UserId,
                    StandardCurrency = 0,
                    PremiumCurrency = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PlayerProfiles.Add(profile);
            }

            // Mặc định nạp 10,000 VND -> 100 Gems (Tỷ lệ 100 VND = 1 Gem)
            int gemsToAdd = transaction.Amount / 100;
            if (gemsToAdd <= 0) gemsToAdd = 10;

            if (transaction.ShopItemId.HasValue)
            {
                var shopItem = await _context.ShopItems.FindAsync(transaction.ShopItemId.Value);
                if (shopItem != null)
                {
                    if (shopItem.ItemType == "CURRENCY" && shopItem.CurrencyType == "GEMS")
                    {
                        gemsToAdd = shopItem.Price;
                    }
                }
            }

            if (transaction.CurrencyType == "COINS")
            {
                profile.StandardCurrency += transaction.Amount;
                _logger.LogInformation($"[PayOSService] Cong {transaction.Amount} Coins cho User {transaction.UserId}. StandardCurrency hien tai: {profile.StandardCurrency}");
            }
            else
            {
                profile.PremiumCurrency += gemsToAdd;
                _logger.LogInformation($"[PayOSService] Cong {gemsToAdd} Gems cho User {transaction.UserId}. PremiumCurrency hien tai: {profile.PremiumCurrency}");
            }

            // Tự động mở khóa súng vĩnh viễn vào PlayerWeapons nếu đơn hàng mua súng VIP
            string desc = transaction.ReferenceCode ?? "";
            await AutoUnlockVietQRWeaponAsync(profile.ProfileId, desc, transaction.ShopItemId, transaction.Amount);
        }

        private async Task AutoUnlockVietQRWeaponAsync(int profileId, string description, int? shopItemId, int amount = 0)
        {
            WeaponConfig? weapon = null;

            if (shopItemId.HasValue)
            {
                var shopItem = await _context.ShopItems.FindAsync(shopItemId.Value);
                if (shopItem != null)
                {
                    weapon = await _context.WeaponConfigs
                        .FirstOrDefaultAsync(w => w.WeaponName.ToLower() == shopItem.Name.ToLower() 
                                               || w.PrefabName.ToLower().Contains(shopItem.Name.ToLower()) 
                                               || shopItem.Name.ToLower().Contains(w.WeaponName.ToLower()));
                }
            }

            string descLower = (description ?? "").ToLower();
            if (weapon == null && (!string.IsNullOrEmpty(descLower) || amount == 2000))
            {
                if (descLower.Contains("missile"))
                    weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("Missile"));
                else if (descLower.Contains("rocket") || descLower.Contains("bazooka"))
                    weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("Rocket"));
                else if (descLower.Contains("ak") || descLower.Contains("gold") || amount == 2000)
                    weapon = await _context.WeaponConfigs.FirstOrDefaultAsync(w => w.PrefabName.Contains("AK_47A_Gold") || w.WeaponName.Contains("AK"));
            }

            // Nếu DB chưa có bản ghi WeaponConfig cho các loại vũ khí này, tự động khởi tạo luôn để đảm bảo không bị NULL
            if (weapon == null && (!string.IsNullOrEmpty(descLower) || amount == 2000))
            {
                if (descLower.Contains("missile"))
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
                else if (descLower.Contains("rocket") || descLower.Contains("bazooka"))
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
                else if (descLower.Contains("ak") || descLower.Contains("gold") || amount == 2000)
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
                var playerWeapon = await _context.PlayerWeapons
                    .FirstOrDefaultAsync(pw => pw.ProfileId == profileId && pw.WeaponConfigId == weapon.Id);

                if (playerWeapon == null)
                {
                    playerWeapon = new PlayerWeapon
                    {
                        ProfileId = profileId,
                        WeaponConfigId = weapon.Id,
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
                _logger.LogInformation($"[PayOSService] Mo khoa thanh cong vu khi '{weapon.WeaponName}' ({weapon.PrefabName}) cho Profile {profileId}!");
            }
        }

        private static string GetJsonCode(JsonElement element)
        {
            if (element.TryGetProperty("code", out var codeProp))
            {
                if (codeProp.ValueKind == JsonValueKind.String) return codeProp.GetString() ?? "";
                if (codeProp.ValueKind == JsonValueKind.Number) return codeProp.GetInt32().ToString();
            }
            return "";
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
