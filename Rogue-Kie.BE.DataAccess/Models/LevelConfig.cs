using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class LevelConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int FloorNumber { get; set; }
        public float DifficultyMultiplier { get; set; }
        public int MaxEnemiesToSpawn { get; set; }
    }
}
