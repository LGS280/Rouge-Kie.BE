namespace Rogue_Kie.BE.Contracts.Users
{
    public class UserResponse
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? RoleName { get; set; }

        public bool isActive { get; set; }
    }
}
