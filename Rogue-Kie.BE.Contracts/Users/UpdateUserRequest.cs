using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Users
{
    public class UpdateUserRequest
    {
        [MaxLength(50)]
        public string? Username { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Password { get; set; }

        public bool? isActive { get; set; }
    }
}
