using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class PlayerCosmetic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(PlayerProfile))]
        public int ProfileId { get; set; }

        [ForeignKey(nameof(CosmeticItem))]
        public int CosmeticId { get; set; }

        public bool IsEquipped { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public PlayerProfile? PlayerProfile { get; set; }
        public CosmeticItem? CosmeticItem { get; set; }
    }
}
