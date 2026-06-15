using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> CreateRoleAsync(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Role name không được để trống.");
            }

            if (name.Length < 2 || name.Length > 50)
            {
                throw new ArgumentException("Role name phải từ 2 đến 50 ký tự.");
            }

            var existingRole = await _context.Roles
                .AnyAsync(x => x.Name == name);

            if (existingRole)
            {
                throw new InvalidOperationException("Role này đã tồn tại.");
            }

            var role = new Role
            {
                Name = name.Trim(),
                Description = description?.Trim()
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return role;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}
