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

        public int BaseRoomCount { get; set; } = 7;
        public int CoopExtraRooms { get; set; } = 2;
        public float CoopMobHPMultiplier { get; set; } = 0.4f;
        public float CoopBossHPMultiplier { get; set; } = 0.6f;
        public int CoopExtraMobsPerRoom { get; set; } = 1;

        public int ChestRoomCount { get; set; } = 1;
        public int CoopExtraChestRooms { get; set; } = 0;
    }

    public class UpdateLevelRequest : CreateLevelRequest
    {
    }
}
