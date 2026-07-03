using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class BuffConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BuffName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string IconPath { get; set; }

        [MaxLength(50)]
        public string BuffType { get; set; }

        public float Value { get; set; }

        [MaxLength(20)]
        public string Rarity { get; set; }
    }
}
