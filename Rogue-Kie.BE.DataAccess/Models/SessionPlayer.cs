using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class SessionPlayer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(GameSession))]
        public int SessionId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(Character))]
        public int CharacterId { get; set; }

        public bool IsHost { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public int CurrentMana { get; set; }

        // Navigation properties
        public GameSession? GameSession { get; set; }
        public User? User { get; set; }
        public Character? Character { get; set; }
    }
}
