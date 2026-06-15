using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Users
{
    public class CreateUserRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}
