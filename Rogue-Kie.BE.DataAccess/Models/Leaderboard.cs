using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class Leaderboard
    {
        [Key]
        [Column("LeaderboardId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeaderboardId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public int HighestWave { get; set; }

        public int TotalKills { get; set; }

        public int TotalRuns { get; set; }

        public int RankPosition { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User? User { get; set; }
    }
}
