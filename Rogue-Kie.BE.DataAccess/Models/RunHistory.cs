using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class RunHistory
    {
        [Key]
        [Column("RunId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RunId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(GameSession))]
        public int SessionId { get; set; }

        [ForeignKey(nameof(Character))]
        public int CharacterId { get; set; }

        public int WavesSurvived { get; set; }

        public int EnemiesKilled { get; set; }

        public int DamageDealt { get; set; }

        public int CurrencyEarned { get; set; }

        public int DurationSeconds { get; set; }

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public GameSession? GameSession { get; set; }
        public Character? Character { get; set; }
    }
}
