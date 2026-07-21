using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class Character
    {
        [Key]
        [Column("CharacterId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CharacterId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int BaseHealth { get; set; }

        public int BaseDamage { get; set; }

        [MaxLength(255)]
        public string? SkillSet { get; set; }

        public int UnlockPrice { get; set; }

        [MaxLength(50)]
        public string? CurrencyType { get; set; }
    }
}
