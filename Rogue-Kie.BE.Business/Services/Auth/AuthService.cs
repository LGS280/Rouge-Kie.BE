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

            // Chống spam gửi OTP: cùng một email chỉ được gửi lại sau cooldown.
            var latestOtp = await _context.RegistrationOtps
                .Where(x => x.Email == normalizedEmail)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestOtp != null &&
                DateTime.UtcNow - latestOtp.CreatedAt < TimeSpan.FromSeconds(OtpResendCooldownSeconds))
            {
                throw new InvalidOperationException("Vui lòng đợi 60 giây trước khi gửi lại mã OTP.");
            }

            var otpCode = GenerateOtpCode();
            var now = DateTime.UtcNow;

            // Mỗi email chỉ giữ OTP mới nhất để tránh nhập nhầm mã cũ.
            var existingOtps = await _context.RegistrationOtps
                .Where(x => x.Email == normalizedEmail)
                .ToListAsync();

            if (existingOtps.Count > 0)
            {
                _context.RegistrationOtps.RemoveRange(existingOtps);
            }

            _context.RegistrationOtps.Add(new RegistrationOtp
            {
                Email = normalizedEmail,
                OtpCode = otpCode,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(OtpExpiryMinutes)
            });

            await _context.SaveChangesAsync();
            await _emailService.SendRegisterOtpAsync(normalizedEmail, otpCode);
        }

        // Đăng ký tài khoản mới sau khi user đã nhận OTP qua email.
        // Nếu lỗi ở đây, kiểm tra validate input, OTP trong DB, role Player và unique username/email.
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

            if (string.IsNullOrWhiteSpace(otpCode))
            {
                throw new ArgumentException("Mã OTP không được để trống.");
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

            // Lấy OTP mới nhất của email để đối chiếu mã người dùng nhập từ Unity.
            var otpRecord = await _context.RegistrationOtps
                .Where(x => x.Email == normalizedEmail)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                throw new InvalidOperationException("Không tìm thấy mã OTP. Vui lòng gửi lại mã OTP.");
            }

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Mã OTP đã hết hạn. Vui lòng gửi lại mã OTP.");
            }

            if (!string.Equals(otpRecord.OtpCode, otpCode.Trim(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Mã OTP không chính xác.");
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
                Role = playerRole
            };

            // Đăng ký xong thì xóa OTP đã dùng để không thể dùng lại mã cũ.
            _context.Users.Add(user);
            _context.RegistrationOtps.Remove(otpRecord);
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
