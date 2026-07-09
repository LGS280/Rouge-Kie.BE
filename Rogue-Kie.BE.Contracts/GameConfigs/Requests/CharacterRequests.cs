using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateCharacterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int BaseHealth { get; set; }

        [Required]
        public int BaseDamage { get; set; }

        [MaxLength(255)]
        public string? SkillSet { get; set; }

        [Required]
        public int UnlockPrice { get; set; }

        [MaxLength(50)]
        public string? CurrencyType { get; set; }
    }

    public class UpdateCharacterRequest : CreateCharacterRequest
    {
    }
}
