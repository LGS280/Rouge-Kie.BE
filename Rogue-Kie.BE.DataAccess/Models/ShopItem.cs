using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class ShopItem
    {
        [Key]
        [Column("ShopItemId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShopItemId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ItemType { get; set; }

        public string? Description { get; set; }

        public int Price { get; set; }

        [MaxLength(50)]
        public string? CurrencyType { get; set; }
    }
}
