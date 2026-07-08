using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class GameSession
    {
        [Key]
        [Column("SessionId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionId { get; set; }

        [ForeignKey(nameof(HostUser))]
        public int HostUserId { get; set; }

        [MaxLength(50)]
        public string? RoomCode { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public int MaxPlayers { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        // Navigation property
        public User? HostUser { get; set; }
    }
}
