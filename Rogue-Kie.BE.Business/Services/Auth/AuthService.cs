using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rogue_Kie.BE.Business.Services.Email;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System.Security.Cryptography;
using System.Text;

namespace Rogue_Kie.BE.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private const int OtpExpiryMinutes = 5;
        private const int OtpResendCooldownSeconds = 60;

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _config = config;
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> _otpCooldowns = new();

        // Táº¡o vÃ  gá»­i OTP cho email Ä‘Äƒng kÃ½.
        // Náº¿u lá»—i á»Ÿ chá»©c nÄƒng nÃ y, kiá»ƒm tra email Ä‘Ã£ tá»“n táº¡i, cooldown OTP vÃ  cáº¥u hÃ¬nh SMTP.
        public async Task SendRegisterOtpAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email khÃ´ng há»£p lá»‡.");
            }

            if (_otpCooldowns.TryGetValue(normalizedEmail, out var lastSent))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSent).TotalSeconds;
                if (secondsSinceLastSent < OtpResendCooldownSeconds)
                {
                    var waitTime = OtpResendCooldownSeconds - (int)secondsSinceLastSent;
                    throw new InvalidOperationException($"Vui lÃ²ng Ä‘á»£i {waitTime} giÃ¢y trÆ°á»›c khi yÃªu cáº§u gá»­i láº¡i OTP.");
                }
            }

            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == normalizedEmail);

            if (emailExists)
            {
                throw new InvalidOperationException("Email Ä‘Ã£ Ä‘Æ°á»£c sá»­ dá»¥ng.");
            }

            // OTP via DB is deprecated by new DBML. Just send mock OTP or bypass.
            // For now, we simulate OTP sending success without database logging.
            var otpCode = GenerateOtpCode();
            await _emailService.SendRegisterOtpAsync(normalizedEmail, otpCode);

            // Record the time OTP was sent for rate limiting
            _otpCooldowns[normalizedEmail] = DateTime.UtcNow;
        }

        // ÄÄƒng kÃ½ tÃ i khoáº£n má»›i trá»±c tiáº¿p khÃ´ng cáº§n lÆ°u/Ä‘á»‘i chiáº¿u OTP qua DB.
                private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _resetOtps = new();

        public async Task SendForgotPasswordOtpAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email không hợp lệ.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
            {
                // Để bảo mật, không báo lỗi rõ ràng nếu email không tồn tại.
                return;
            }

            if (_otpCooldowns.TryGetValue(normalizedEmail, out var lastSent))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSent).TotalSeconds;
                if (secondsSinceLastSent < OtpResendCooldownSeconds)
                {
                    var waitTime = OtpResendCooldownSeconds - (int)secondsSinceLastSent;
                    throw new InvalidOperationException($"Vui lòng đợi {waitTime} giây trước khi yêu cầu gửi lại OTP.");
                }
            }

            var otpCode = GenerateOtpCode();
            _resetOtps[normalizedEmail] = (otpCode, DateTime.UtcNow.AddMinutes(OtpExpiryMinutes));
            
            await _emailService.SendForgotPasswordOtpAsync(normalizedEmail, otpCode);
            _otpCooldowns[normalizedEmail] = DateTime.UtcNow;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otpCode, string newPassword)
        {
            var normalizedEmail = NormalizeEmail(email);

            if (!_resetOtps.TryGetValue(normalizedEmail, out var resetData))
            {
                throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            if (resetData.Expiry < DateTime.UtcNow)
            {
                _resetOtps.TryRemove(normalizedEmail, out _);
                throw new InvalidOperationException("Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
            }

            if (resetData.Otp != otpCode)
            {
                throw new InvalidOperationException("Mã OTP không chính xác.");
            }

            if (newPassword.Length < 6 || newPassword.Length > 255)
            {
                throw new ArgumentException("Mật khẩu mới phải từ 6 đến 255 ký tự.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
            {
                throw new InvalidOperationException("Tài khoản không tồn tại.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Xóa OTP sau khi dùng thành công
            _resetOtps.TryRemove(normalizedEmail, out _);

            return true;
        }

        public async Task<User?> RegisterAsync(string username, string email, string password, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username vÃ  Password khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
            }

            var normalizedEmail = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email khÃ´ng há»£p lá»‡.");
            }

            if (username.Length < 3 || username.Length > 50)
            {
                throw new ArgumentException("Username pháº£i tá»« 3 Ä‘áº¿n 50 kÃ½ tá»±.");
            }

            if (password.Length < 6 || password.Length > 255)
            {
                throw new ArgumentException("Password pháº£i tá»« 6 Ä‘áº¿n 255 kÃ½ tá»±.");
            }

            // KhÃ´ng cho trÃ¹ng username hoáº·c email vÃ¬ cáº£ hai Ä‘á»u dÃ¹ng Ä‘á»ƒ Ä‘á»‹nh danh Ä‘Äƒng nháº­p.
            var existingUser = await _context.Users
                .AnyAsync(x => x.Username == username || x.Email == normalizedEmail);

            if (existingUser)
            {
                throw new InvalidOperationException("Username hoáº·c Email Ä‘Ã£ tá»“n táº¡i.");
            }

            var playerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Player");

            if (playerRole == null)
            {
                throw new InvalidOperationException("Role Player khÃ´ng tá»“n táº¡i.");
            }

            // LÆ°u password Ä‘Ã£ hash, khÃ´ng lÆ°u password gá»‘c vÃ o database.
            var user = new User
            {
                Username = username.Trim(),
                Email = normalizedEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = playerRole.Id,
                Role = playerRole,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // ÄÄƒng nháº­p báº±ng username hoáº·c email.
        // Unity váº«n gá»­i field "username", nhÆ°ng giÃ¡ trá»‹ cÃ³ thá»ƒ lÃ  username hoáº·c email.
        // Dang nhap bang Google/Gmail. Unity gui Google ID token len backend de verify.
        public async Task<User?> LoginWithGoogleAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new ArgumentException("Google ID token khong duoc de trong.");
            }

            var clientIds = GetGoogleClientIds();
            if (clientIds.Count == 0)
            {
                throw new InvalidOperationException("GoogleAuth:ClientId hoac GoogleAuth:ClientIds chua duoc cau hinh.");
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken.Trim(), new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = clientIds
                });
            }
            catch (InvalidJwtException)
            {
                throw new InvalidOperationException("Google ID token khong hop le.");
            }

            if (!payload.EmailVerified)
            {
                throw new InvalidOperationException("Email Google chua duoc xac thuc.");
            }

            var normalizedEmail = NormalizeEmail(payload.Email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new InvalidOperationException("Google token khong co email hop le.");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user != null)
            {
                // Báº¢O Vá»† NGHá»†M NGáº T: Kiá»ƒm tra cá» IsActive cá»§a tÃ i khoáº£n.
                // Náº¿u tÃ i khoáº£n bá»‹ Admin chuyá»ƒn IsActive = false (hoáº·c Soft Delete), láº­p tá»©c cháº·n khÃ´ng cho cáº¥p JWT Token.
                if (!user.IsActive)
                {
                    throw new InvalidOperationException("TÃ i khoáº£n cá»§a báº¡n Ä‘Ã£ bá»‹ khÃ³a do vi pháº¡m quy Ä‘á»‹nh.");
                }

                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return user;
            }

            var playerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Player");

            if (playerRole == null)
            {
                throw new InvalidOperationException("Role Player khong ton tai.");
            }

            user = new User
            {
                Username = await GenerateUniqueGoogleUsernameAsync(payload.Name, normalizedEmail),
                Email = normalizedEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))),
                RoleId = playerRole.Id,
                Role = playerRole,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> LoginAsync(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username/Email vÃ  Password khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng.");
            }

            // Email trong DB Ä‘Æ°á»£c lÆ°u lowercase, nÃªn cáº§n normalize trÆ°á»›c khi so sÃ¡nh.
            var loginIdentifier = usernameOrEmail.Trim();
            var normalizedEmail = NormalizeEmail(loginIdentifier);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Username == loginIdentifier || x.Email == normalizedEmail);

            if (user == null)
            {
                throw new InvalidOperationException("Username hoáº·c Email khÃ´ng tá»“n táº¡i.");
            }

            // Báº¢O Vá»† NGHá»†M NGáº T: Kiá»ƒm tra cá» IsActive trÆ°á»›c khi cho phÃ©p Ä‘Äƒng nháº­p.
            // NgÄƒn cháº·n tÃ i khoáº£n bá»‹ Admin khÃ³a (Block/Lock) tiáº¿p tá»¥c truy cáº­p há»‡ thá»‘ng.
            if (!user.IsActive)
            {
                throw new InvalidOperationException("Your account has been suspended by an Administrator.");
            }

            // So sÃ¡nh password nháº­p vÃ o vá»›i password hash trong database.
            var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("Password khÃ´ng chÃ­nh xÃ¡c.");
            }

            return user;
        }

        // Chuáº©n hÃ³a email Ä‘á»ƒ trÃ¡nh lá»—i khÃ¡c chá»¯ hoa/thÆ°á»ng hoáº·c dÆ° khoáº£ng tráº¯ng.
        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        // Táº¡o mÃ£ OTP 6 chá»¯ sá»‘.
        private static string GenerateOtpCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        private List<string> GetGoogleClientIds()
        {
            var clientIds = _config.GetSection("GoogleAuth:ClientIds")
                .GetChildren()
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .ToList();

            var singleClientId = _config["GoogleAuth:ClientId"];
            if (!string.IsNullOrWhiteSpace(singleClientId))
            {
                clientIds.Add(singleClientId.Trim());
            }

            return clientIds.Distinct(StringComparer.Ordinal).ToList();
        }

        private async Task<string> GenerateUniqueGoogleUsernameAsync(string? displayName, string email)
        {
            var source = string.IsNullOrWhiteSpace(displayName)
                ? email.Split('@')[0]
                : displayName;

            var baseUsername = SanitizeUsername(source);
            var username = baseUsername;
            var suffix = 1;

            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                var suffixText = suffix.ToString();
                var maxBaseLength = Math.Max(1, 50 - suffixText.Length);
                username = baseUsername.Length > maxBaseLength
                    ? baseUsername[..maxBaseLength] + suffixText
                    : baseUsername + suffixText;
                suffix++;
            }

            return username;
        }

        private static string SanitizeUsername(string value)
        {
            var builder = new StringBuilder();

            foreach (var c in value.Trim())
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else if (c == '_' || c == '-' || c == '.')
                {
                    builder.Append(c);
                }
            }

            var username = builder.ToString();
            if (username.Length < 3)
            {
                username = "google_user";
            }

            return username.Length > 50 ? username[..50] : username;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            var tokenString = Convert.ToBase64String(randomNumber);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = tokenString,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Revoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<User?> VerifyRefreshTokenAsync(string token)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.Revoked);

            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            if (storedToken.User != null && !storedToken.User.IsActive)
            {
                return null;
            }

            // Thu hồi token cũ (Xoay vòng Refresh Token)
            storedToken.Revoked = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            return storedToken.User;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.Revoked);

            if (storedToken == null)
            {
                return false;
            }

            storedToken.Revoked = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

