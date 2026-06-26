using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Auth
{
    public interface IAuthService
    {
        Task SendRegisterOtpAsync(string email);

        Task<User?> RegisterAsync(string username, string email, string password, string otpCode);

        Task<User?> LoginAsync(string usernameOrEmail, string password);
    }
}
