namespace Rogue_Kie.BE.Business.Services.Auth
{
    public interface ITokenService
    {
        string GenerateToken(DataAccess.Models.User user);
    }
}
