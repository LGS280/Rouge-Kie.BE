using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateBuffRequest
    {
        [Required]
        [MaxLength(50)]
        public string BuffName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BuffType { get; set; } = string.Empty;

        [Required]
        public float Value { get; set; }

        [Required]
        [MaxLength(20)]
        public string Rarity { get; set; } = string.Empty;
    }

    public class UpdateBuffRequest : CreateBuffRequest
    {
    }
}
