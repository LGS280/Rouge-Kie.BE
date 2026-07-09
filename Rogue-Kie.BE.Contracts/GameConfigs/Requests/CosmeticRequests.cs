using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateCosmeticRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(50)]
        public string? Rarity { get; set; }

        [Required]
        public int Price { get; set; }

        [MaxLength(50)]
        public string? CurrencyType { get; set; }
    }

    public class UpdateCosmeticRequest : CreateCosmeticRequest
    {
    }
}
