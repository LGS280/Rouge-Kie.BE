using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class RoomWaveConfig
    {
        [Key]
        [Column("RoomWaveConfigId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomWaveConfigId { get; set; }

        [ForeignKey(nameof(RoomConfig))]
        public int RoomConfigId { get; set; }

        public int WaveNumber { get; set; }

        public int MaxSimultaneousMonsters { get; set; }

        // Navigation property
        public RoomConfig? RoomConfig { get; set; }
    }
}
