using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.Users;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> CreateUserAsync(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username, Email và Password không được để trống.");
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .AnyAsync(x => x.Username == username.Trim() || x.Email == normalizedEmail);

            if (existingUser)
            {
                throw new InvalidOperationException("Username hoặc Email đã tồn tại.");
            }

            var user = new User
            {
                Username = username.Trim(),
                Email = normalizedEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleName = u.Role != null ? u.Role.Name : null,
                    isActive = u.IsActive
                })
                .ToListAsync();
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var u = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (u == null) return null;

            return new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                RoleName = u.Role != null ? u.Role.Name : null,
                isActive = u.IsActive
            };
        }

        public async Task<UserResponse?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var u = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == username.Trim());

            if (u == null) return null;

            return new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                RoleName = u.Role != null ? u.Role.Name : null,
                isActive = u.IsActive
            };
        }

        public async Task<UserResponse> UpdateUserAsync(int id, string? username, string? email, string? password, bool? isActive)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new KeyNotFoundException("User không tồn tại.");

            if (!string.IsNullOrWhiteSpace(username) && username.Trim() != user.Username)
            {
                var existsUsername = await _context.Users.AnyAsync(u => u.Username == username.Trim() && u.Id != id);
                if (existsUsername) throw new InvalidOperationException("Username đã tồn tại.");
                user.Username = username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                if (normalizedEmail != user.Email)
                {
                    var existsEmail = await _context.Users.AnyAsync(u => u.Email == normalizedEmail && u.Id != id);
                    if (existsEmail) throw new InvalidOperationException("Email đã tồn tại.");
                    user.Email = normalizedEmail;
                }
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            }

            if (isActive.HasValue)
            {
                user.IsActive = isActive.Value;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role != null ? user.Role.Name : null,
                isActive = user.IsActive
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            // Soft delete: set inactive instead of removing row
            user.IsActive = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}