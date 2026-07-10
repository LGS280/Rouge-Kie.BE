namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class ShopItemResponse
    {
        public int ShopItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ItemType { get; set; }
        public string? Description { get; set; }
        public int Price { get; set; }
        public string? CurrencyType { get; set; }
    }
}
