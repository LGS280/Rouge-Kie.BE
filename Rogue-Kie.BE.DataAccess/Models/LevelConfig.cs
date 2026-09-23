using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class LevelConfig
    {
        [Key]
        [Column("LevelConfigId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StageId { get; set; }
        public int FloorNumber { get; set; }
        public float DifficultyMultiplier { get; set; }

        public int BaseRoomCount { get; set; } = 7;
        public int CoopExtraRooms { get; set; } = 2;
        public float CoopMobHPMultiplier { get; set; } = 0.4f;
        public float CoopBossHPMultiplier { get; set; } = 0.6f;
        public int CoopExtraMobsPerRoom { get; set; } = 1;
    }
}
