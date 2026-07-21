namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class BulletResponse
    {
        public int Id { get; set; }
        public string BulletName { get; set; } = string.Empty;
        public int Damage { get; set; }
        public float CritRate { get; set; }
        public float CritMultiplier { get; set; }
        public float FlightSpeed { get; set; }
        public int PiercingCount { get; set; }
        public string PrefabName { get; set; } = string.Empty;
    }
}
