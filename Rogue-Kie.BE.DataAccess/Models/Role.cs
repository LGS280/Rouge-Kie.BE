using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        // Navigation property
        public ICollection<User>? Users { get; set; }
    }
}
