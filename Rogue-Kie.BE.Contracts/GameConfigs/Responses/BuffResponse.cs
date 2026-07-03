namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class BuffResponse
    {
        public int Id { get; set; }
        public string BuffName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BuffType { get; set; } = string.Empty;
        public float Value { get; set; }
        public string Rarity { get; set; } = string.Empty;
    }
}
