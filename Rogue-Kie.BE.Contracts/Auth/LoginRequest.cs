using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Auth
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;
    }
}
