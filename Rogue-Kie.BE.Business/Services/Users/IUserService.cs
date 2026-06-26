using Rogue_Kie.BE.Contracts.Users;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Users
{
    public interface IUserService
    {
        Task<User?> CreateUserAsync(string username, string email, string password);
        Task<List<UserResponse>> GetAllUsersAsync();
    }
}
