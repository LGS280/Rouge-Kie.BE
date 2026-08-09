namespace Rogue_Kie.BE.Contracts.Payment
{
    public class CreatePaymentLinkRequest
    {
        public int? ShopItemId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; } = "Rogue-Kie Payment";
        public string CurrencyType { get; set; } = "GEMS";
    }
}
