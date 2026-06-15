using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Roles
{
    public class CreateRoleRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
