using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class CosmeticItem
    {
        [Key]
        [Column("CosmeticId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CosmeticId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(50)]
        public string? Rarity { get; set; }

        public int Price { get; set; }

        [MaxLength(50)]
        public string? CurrencyType { get; set; }
    }
}
