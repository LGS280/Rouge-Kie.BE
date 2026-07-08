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
    }
}
