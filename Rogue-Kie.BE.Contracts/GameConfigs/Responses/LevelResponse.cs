namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class LevelResponse
    {
        public int Id { get; set; }
        public int FloorNumber { get; set; }
        public int MaxEnemiesToSpawn { get; set; }
        public float DifficultyMultiplier { get; set; }
    }
}
