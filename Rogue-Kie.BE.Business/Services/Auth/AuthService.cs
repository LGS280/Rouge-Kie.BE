using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username và Password không được để trống.");
            }

            if (username.Length < 3 || username.Length > 50)
            {
                throw new ArgumentException("Username phải từ 3 đến 50 ký tự.");
            }

            if (password.Length < 6 || password.Length > 255)
            {
                throw new ArgumentException("Password phải từ 6 đến 255 ký tự.");
            }

            var existingUser = await _context.Users
                .AnyAsync(x => x.Username == username);

            if (existingUser)
            {
                throw new InvalidOperationException("Username đã tồn tại.");
            }

            var user = new User
            {
                Username = username.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username và Password không được để trống.");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
            {
                throw new InvalidOperationException("Username không tồn tại.");
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!passwordValid)
            {
                throw new InvalidOperationException("Password không chính xác.");
            }

            return user;
        }
    }
}
