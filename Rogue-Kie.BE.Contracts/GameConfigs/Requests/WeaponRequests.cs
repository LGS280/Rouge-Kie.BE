using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateWeaponRequest
    {
        [Required]
        [MaxLength(50)]
        public string WeaponName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PrefabName { get; set; } = string.Empty;

        [Required]
        public float FireRate { get; set; }

        [Required]
        public int ManaCost { get; set; }

        [Required]
        public int BulletsPerShot { get; set; }

        [Required]
        public float SpreadAngle { get; set; }

        [MaxLength(50)]
        public string ShootSound { get; set; } = string.Empty;
        public float ShootVolume { get; set; } = 1.0f;

        public float HandPositionX { get; set; } = 0f;
        public float HandPositionY { get; set; } = 0f;
        public float HandPositionZ { get; set; } = 0f;

        public float RecoilDistance { get; set; } = 0.15f;
        public float RecoilDuration { get; set; } = 0.05f;
        public float ReturnDuration { get; set; } = 0.1f;

        [Required]
        public int BulletId { get; set; }
    }

    public class UpdateWeaponRequest : CreateWeaponRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
