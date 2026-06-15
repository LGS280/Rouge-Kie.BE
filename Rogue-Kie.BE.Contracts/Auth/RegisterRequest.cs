using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Auth
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(255)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
