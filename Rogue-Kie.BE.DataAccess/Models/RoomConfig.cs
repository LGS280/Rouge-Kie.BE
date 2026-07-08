using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class RoomConfig
    {
        [Key]
        [Column("RoomConfigId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomConfigId { get; set; }

        [ForeignKey(nameof(LevelConfig))]
        public int LevelConfigId { get; set; }

        [MaxLength(50)]
        public string? RoomType { get; set; }

        public int TotalWavesInRoom { get; set; }

        // Navigation property
        public LevelConfig? LevelConfig { get; set; }
    }
}
