using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateLevelRequest
    {
        [Required]
        public int FloorNumber { get; set; }

        [Required]
        public int MaxEnemiesToSpawn { get; set; }

        [Required]
        public float DifficultyMultiplier { get; set; }
    }

    public class UpdateLevelRequest : CreateLevelRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
