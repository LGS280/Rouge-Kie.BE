using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Auth
{
    public class SendRegisterOtpRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }
}
