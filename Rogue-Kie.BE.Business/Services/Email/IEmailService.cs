namespace Rogue_Kie.BE.Business.Services.Email
{
    public interface IEmailService
    {
        Task SendRegisterOtpAsync(string toEmail, string otpCode);
    }
}
