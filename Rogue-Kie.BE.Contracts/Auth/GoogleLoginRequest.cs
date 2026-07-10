using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
