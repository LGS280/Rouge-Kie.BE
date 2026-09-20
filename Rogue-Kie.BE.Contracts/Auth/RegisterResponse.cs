namespace Rogue_Kie.BE.Contracts.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }

        public bool IsMaintenance { get; set; } = false;

        public Rogue_Kie.BE.Contracts.Maintenance.CurrentMaintenanceStatusResponse? Maintenance { get; set; }
    }
}
