using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Roles
{
    public interface IRoleService
    {
        Task<Role?> CreateRoleAsync(string name, string? description);

        Task<List<Role>> GetAllRolesAsync();
    }
}
