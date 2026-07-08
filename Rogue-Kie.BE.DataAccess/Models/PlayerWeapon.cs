using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class PlayerWeapon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(PlayerProfile))]
        public int ProfileId { get; set; }

        [ForeignKey(nameof(WeaponConfig))]
        public int WeaponConfigId { get; set; }

        public bool IsUnlocked { get; set; }

        public DateTime? UnlockedAt { get; set; }

        // Navigation properties
        public PlayerProfile? PlayerProfile { get; set; }
        public WeaponConfig? WeaponConfig { get; set; }
    }
}
