using Rogue_Kie.BE.DataAccess.Models;
using Rogue_Kie.BE.Contracts.Auth;

namespace Rogue_Kie.BE.Business.Services.Auth
{
    public interface IAuthService
    {
        Task SendRegisterOtpAsync(string email);

        Task<User?> RegisterAsync(string username, string email, string password, string otpCode);

        Task<User?> LoginAsync(string usernameOrEmail, string password);

        Task<User?> VerifyRefreshTokenAsync(string token);

        Task<bool> RevokeRefreshTokenAsync(string token);

        Task<RefreshToken> GenerateRefreshTokenAsync(int userId);
    }
}
