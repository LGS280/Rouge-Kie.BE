using System;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class BuyItemResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ShopItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int RemainingStandardCurrency { get; set; }
        public int RemainingPremiumCurrency { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    }
}
