using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class PlayerCharacter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(PlayerProfile))]
        public int ProfileId { get; set; }

        [ForeignKey(nameof(Character))]
        public int CharacterId { get; set; }

        public bool IsUnlocked { get; set; }

        public DateTime? UnlockedAt { get; set; }

        // Navigation properties
        public PlayerProfile? PlayerProfile { get; set; }
        public Character? Character { get; set; }
    }
}
