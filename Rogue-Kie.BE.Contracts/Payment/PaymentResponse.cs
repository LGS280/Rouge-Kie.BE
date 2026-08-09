using System;

namespace Rogue_Kie.BE.Contracts.Payment
{
    public class PaymentResponse
    {
        public int TransactionId { get; set; }
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING";
        public string CurrencyType { get; set; } = "VND";
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
