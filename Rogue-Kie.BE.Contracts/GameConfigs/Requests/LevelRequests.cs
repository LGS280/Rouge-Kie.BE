using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateLevelRequest
    {
        [Required]
        public int StageId { get; set; }

        [Required]
        public int FloorNumber { get; set; }

        [Required]
        public float DifficultyMultiplier { get; set; }
    }

    public class UpdateLevelRequest : CreateLevelRequest
    {
    }
}
