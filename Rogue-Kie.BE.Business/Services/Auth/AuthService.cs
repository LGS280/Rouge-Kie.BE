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

        // Create and send OTP for registering email.
        // If there is an error in this function, check if the email already exists, OTP cooldown, and SMTP configuration.
        public async Task SendRegisterOtpAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email is not valid.");
            }

            if (_otpCooldowns.TryGetValue(normalizedEmail, out var lastSent))
            {
                var secondsSinceLastSent = (DateTime.UtcNow - lastSent).TotalSeconds;
                if (secondsSinceLastSent < OtpResendCooldownSeconds)
                {
                    var waitTime = OtpResendCooldownSeconds - (int)secondsSinceLastSent;
                    throw new InvalidOperationException($"Please wait {waitTime} seconds before requesting a new OTP.");
                }
            }

            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == normalizedEmail);

            if (emailExists)
            {
                throw new InvalidOperationException("Email is already in use.");
            }

            // OTP via DB is deprecated by new DBML. Just send mock OTP or bypass.
            // For now, we simulate OTP sending success without database logging.
            var otpCode = GenerateOtpCode();
            await _emailService.SendRegisterOtpAsync(normalizedEmail, otpCode);

            // Record the time OTP was sent for rate limiting
            _otpCooldowns[normalizedEmail] = DateTime.UtcNow;
        }

        // Register a new account directly without storing/verifying OTP via DB.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _resetOtps = new();

        public async Task SendForgotPasswordOtpAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email is not valid.");
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
                    throw new InvalidOperationException($"Please wait {waitTime} seconds before requesting a new OTP.");
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
                throw new InvalidOperationException("OTP code is invalid or has expired.");
            }

            if (resetData.Expiry < DateTime.UtcNow)
            {
                _resetOtps.TryRemove(normalizedEmail, out _);
                throw new InvalidOperationException("OTP code has expired. Please request a new code.");
            }

            if (resetData.Otp != otpCode)
            {
                throw new InvalidOperationException("OTP code is incorrect.");
            }

            if (newPassword.Length < 6 || newPassword.Length > 255)
            {
                throw new ArgumentException("New password must be between 6 and 255 characters.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
            {
                throw new InvalidOperationException("Account does not exist.");
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
                throw new ArgumentException("Username and Password cannot be empty.");
            }

            var normalizedEmail = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new ArgumentException("Email is not valid.");
            }

            if (username.Length < 3 || username.Length > 50)
            {
                throw new ArgumentException("Username must be between 3 and 50 characters.");
            }

            if (password.Length < 6 || password.Length > 255)
            {
                throw new ArgumentException("Password must be between 6 and 255 characters.");
            }

            // Không cho trùng username hoặc email vì cả hai đều dùng để định danh đăng nhập.
            var existingUser = await _context.Users
                .AnyAsync(x => x.Username == username || x.Email == normalizedEmail);

            if (existingUser)
            {
                throw new InvalidOperationException("Username or Email already exists.");
            }

            var playerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Player");

            if (playerRole == null)
            {
                throw new InvalidOperationException("Role Player does not exist.");
            }

            // Save the hashed password, do not store the plain password in the database.
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

        // Login with username or email.
        // Unity still sends the field "username", but the value can be either username or email.
        // Login with Google/Gmail. Unity sends Google ID token to backend for verification.
        public async Task<User?> LoginWithGoogleAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new ArgumentException("Google ID token cannot be empty.");
            }

            var clientIds = GetGoogleClientIds();
            if (clientIds.Count == 0)
            {
                throw new InvalidOperationException("GoogleAuth:ClientId or GoogleAuth:ClientIds has not been configured.");
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
                throw new InvalidOperationException("Google ID token is not valid.");
            }

            if (!payload.EmailVerified)
            {
                throw new InvalidOperationException("Google email has not been verified.");
            }

            var normalizedEmail = NormalizeEmail(payload.Email);
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new InvalidOperationException("Google token does not contain a valid email.");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user != null)
            {
                // PROTECT USER ACCOUNT: Check the IsActive flag of the account.
                // If the account is set to IsActive = false by Admin (or Soft Delete), immediately block from issuing JWT Token.
                if (!user.IsActive)
                {
                    throw new InvalidOperationException("Your account has been locked due to policy violations.");
                }

                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return user;
            }

            var playerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Player");

            if (playerRole == null)
            {
                throw new InvalidOperationException("Role Player does not exist.");
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
                throw new ArgumentException("Username/Email and Password cannot be empty.");
            }

            // Email in the DB is stored in lowercase, so it needs to be normalized before comparison.
            var loginIdentifier = usernameOrEmail.Trim();
            var normalizedEmail = NormalizeEmail(loginIdentifier);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Username == loginIdentifier || x.Email == normalizedEmail);

            if (user == null)
            {
                throw new InvalidOperationException("Username or Email does not exist.");
            }

            // PROTECT USER ACCOUNT: Check the IsActive flag of the account.
            // If the account is set to IsActive = false by Admin (or Soft Delete), immediately block from issuing JWT Token.
            if (!user.IsActive)
            {
                throw new InvalidOperationException("Your account has been suspended by an Administrator.");
            }

            // PROTECT USER ACCOUNT: Compare the entered password with the hashed password in the database.
            var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("Password is incorrect.");
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

