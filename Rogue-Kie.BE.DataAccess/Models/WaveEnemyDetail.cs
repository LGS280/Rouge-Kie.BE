using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class WaveEnemyDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(RoomWaveConfig))]
        public int RoomWaveConfigId { get; set; }

        [ForeignKey(nameof(EnemyConfig))]
        public int EnemyConfigId { get; set; }

        public int Quantity { get; set; }

        // Navigation properties
        public RoomWaveConfig? RoomWaveConfig { get; set; }
        public EnemyConfig? EnemyConfig { get; set; }
    }
}
