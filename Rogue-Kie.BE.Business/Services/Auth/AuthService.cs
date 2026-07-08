using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Business.Services.Email;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System.Security.Cryptography;

namespace Rogue_Kie.BE.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private const int OtpExpiryMinutes = 5;
        private const int OtpResendCooldownSeconds = 60;

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
    }
}
