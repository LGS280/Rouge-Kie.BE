using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateBulletRequest
    {
        [Required]
        [MaxLength(50)]
        public string BulletName { get; set; } = string.Empty;

        [Required]
        public int Damage { get; set; }

        [Required]
        public float CritRate { get; set; }

        [Required]
        public float FlightSpeed { get; set; }

        [Required]
        public int PiercingCount { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrefabName { get; set; } = string.Empty;
    }

    public class UpdateBulletRequest : CreateBulletRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
