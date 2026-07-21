namespace Rogue_Kie.BE.Contracts.Auth
{
    public class SendRegisterOtpResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
