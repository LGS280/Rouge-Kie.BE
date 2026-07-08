using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class Transaction
    {
        [Key]
        [Column("TransactionId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(ShopItem))]
        public int ShopItemId { get; set; }

        [MaxLength(50)]
        public string? TransactionType { get; set; }

        public int Amount { get; set; }

        [MaxLength(50)]
        public string? CurrencyType { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? ReferenceCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public ShopItem? ShopItem { get; set; }
    }
}
