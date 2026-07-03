using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateWeaponRequest
    {
        [Required]
        [MaxLength(50)]
        public string WeaponName { get; set; } = string.Empty;

        [Required]
        public float FireRate { get; set; }

        [Required]
        public int ManaCost { get; set; }

        [Required]
        public int BulletsPerShot { get; set; }

        [Required]
        public float SpreadAngle { get; set; }

        [Required]
        public int BulletId { get; set; }
    }

    public class UpdateWeaponRequest : CreateWeaponRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
