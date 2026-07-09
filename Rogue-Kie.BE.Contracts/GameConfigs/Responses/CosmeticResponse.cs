namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class CosmeticResponse
    {
        public int CosmeticId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Rarity { get; set; }
        public int Price { get; set; }
        public string? CurrencyType { get; set; }
    }
}
