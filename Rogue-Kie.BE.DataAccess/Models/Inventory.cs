using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class Inventory
    {
        [Key]
        [Column("InventoryId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryId { get; set; }

        [ForeignKey(nameof(PlayerProfile))]
        public int ProfileId { get; set; }

        [ForeignKey(nameof(ShopItem))]
        public int ShopItemId { get; set; }

        public int Quantity { get; set; }

        // Navigation properties
        public PlayerProfile? PlayerProfile { get; set; }
        public ShopItem? ShopItem { get; set; }
    }
}
