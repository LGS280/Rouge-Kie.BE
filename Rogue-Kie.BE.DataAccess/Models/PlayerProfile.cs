using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class PlayerProfile
    {
        [Key]
        [Column("ProfileId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProfileId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string? DisplayName { get; set; }

        public int StandardCurrency { get; set; }

        public int PremiumCurrency { get; set; }

        public int TotalRuns { get; set; }

        public int HighestWave { get; set; }

        public int TotalKills { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User? User { get; set; }
    }
}
