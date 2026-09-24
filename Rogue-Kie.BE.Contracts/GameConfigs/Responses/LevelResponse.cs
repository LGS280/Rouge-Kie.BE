namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class LevelResponse
    {
        public int Id { get; set; }
        public int StageId { get; set; }
        public int FloorNumber { get; set; }
        public float DifficultyMultiplier { get; set; }

        public int BaseRoomCount { get; set; }
        public int CoopExtraRooms { get; set; }
        public float CoopMobHPMultiplier { get; set; }
        public float CoopBossHPMultiplier { get; set; }
        public int CoopExtraMobsPerRoom { get; set; }

        public int ChestRoomCount { get; set; }
        public int CoopExtraChestRooms { get; set; }
    }
}
