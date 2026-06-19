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

        public async Task<User?> CreateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username và Password không được để trống.");
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

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.Id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    RoleName = u.Role != null ? u.Role.Name : null
                })
                .ToListAsync();
        }
    }
}
