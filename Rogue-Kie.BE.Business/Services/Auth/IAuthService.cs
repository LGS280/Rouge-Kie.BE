using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Auth
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(string username, string password);

        Task<User?> LoginAsync(string username, string password);
    }
}
