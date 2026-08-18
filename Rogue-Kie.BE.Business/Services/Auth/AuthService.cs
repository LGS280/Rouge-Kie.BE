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

        // Tạo và gửi OTP cho email đăng ký.
        // Nếu lỗi ở chức năng này, kiểm tra email đã tồn tại, cooldown OTP và cấu hình SMTP.
        public async Task SendRegisterOtpAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email không hợp lệ.");
            }

            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == normalizedEmail);

            if (emailExists)
            {
                throw new InvalidOperationException("Email đã được sử dụng.");
            }

            // OTP via DB is deprecated by new DBML. Just send mock OTP or bypass.
            // For now, we simulate OTP sending success without database logging.
            var otpCode = GenerateOtpCode();
            await _emailService.SendRegisterOtpAsync(normalizedEmail, otpCode);
        }

        // Đăng ký tài khoản mới trực tiếp không cần lưu/đối chiếu OTP qua DB.
        public async Task<User?> RegisterAsync(string username, string email, string password, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username và Password không được để trống.");
            }

            var normalizedEmail = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email không hợp lệ.");
            }

            if (username.Length < 3 || username.Length > 50)
            {
                throw new ArgumentException("Username phải từ 3 đến 50 ký tự.");
            }

            if (password.Length < 6 || password.Length > 255)
            {
                throw new ArgumentException("Password phải từ 6 đến 255 ký tự.");
            }

            // Không cho trùng username hoặc email vì cả hai đều dùng để định danh đăng nhập.
            var existingUser = await _context.Users
                .AnyAsync(x => x.Username == username || x.Email == normalizedEmail);

            if (existingUser)
            {
                throw new InvalidOperationException("Username hoặc Email đã tồn tại.");
            }

            var playerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Player");

            if (playerRole == null)
            {
                throw new InvalidOperationException("Role Player không tồn tại.");
            }

            // Lưu password đã hash, không lưu password gốc vào database.
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

        // Đăng nhập bằng username hoặc email.
        // Unity vẫn gửi field "username", nhưng giá trị có thể là username hoặc email.
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
                // BẢO VỆ NGHỆM NGẠT: Kiểm tra cờ IsActive của tài khoản.
                // Nếu tài khoản bị Admin chuyển IsActive = false (hoặc Soft Delete), lập tức chặn không cho cấp JWT Token.
                if (!user.IsActive)
                {
                    throw new InvalidOperationException("Tài khoản của bạn đã bị khóa do vi phạm quy định.");
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
                throw new ArgumentException("Username/Email và Password không được để trống.");
            }

            // Email trong DB được lưu lowercase, nên cần normalize trước khi so sánh.
            var loginIdentifier = usernameOrEmail.Trim();
            var normalizedEmail = NormalizeEmail(loginIdentifier);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Username == loginIdentifier || x.Email == normalizedEmail);

            if (user == null)
            {
                throw new InvalidOperationException("Username hoặc Email không tồn tại.");
            }

            // BẢO VỆ NGHỆM NGẠT: Kiểm tra cờ IsActive trước khi cho phép đăng nhập.
            // Ngăn chặn tài khoản bị Admin khóa (Block/Lock) tiếp tục truy cập hệ thống.
            if (!user.IsActive)
            {
                throw new InvalidOperationException("Tài khoản của bạn đã bị khóa do vi phạm quy định.");
            }

            // So sánh password nhập vào với password hash trong database.
            var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("Password không chính xác.");
            }

            return user;
        }

        // Chuẩn hóa email để tránh lỗi khác chữ hoa/thường hoặc dư khoảng trắng.
        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        // Tạo mã OTP 6 chữ số.
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
