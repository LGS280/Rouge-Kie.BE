using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class WeaponConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string WeaponName { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrefabName { get; set; } = string.Empty;

        public float FireRate { get; set; }
        public int ManaCost { get; set; }
        public int BulletsPerShot { get; set; }
        public float SpreadAngle { get; set; }

        // Audio Settings
        [MaxLength(50)]
        public string ShootSound { get; set; } = string.Empty;
        public float ShootVolume { get; set; } = 1.0f;

        // Custom Hand Position
        public float HandPositionX { get; set; } = 0f;
        public float HandPositionY { get; set; } = 0f;
        public float HandPositionZ { get; set; } = 0f;

        // Recoil Settings
        public float RecoilDistance { get; set; } = 0.15f;
        public float RecoilDuration { get; set; } = 0.05f;
        public float ReturnDuration { get; set; } = 0.1f;

        [MaxLength(50)]
        public string WeaponType { get; set; } = "Rifle";

        [MaxLength(50)]
        public string Rarity { get; set; } = "Common";

        public int BulletId { get; set; }

        [ForeignKey("BulletId")]
        public BulletConfig BulletConfig { get; set; }

        public int? SecondBulletId { get; set; }

        [ForeignKey("SecondBulletId")]
        public BulletConfig? SecondBulletConfig { get; set; }
    }
}
