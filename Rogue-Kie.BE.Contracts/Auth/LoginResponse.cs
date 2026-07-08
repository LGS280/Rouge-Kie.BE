namespace Rogue_Kie.BE.Contracts.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public string? Username { get; set; }

        public string? Role { get; set; }

        public string? Token { get; set; }

        public string? RefreshToken { get; set; }
    }
}
