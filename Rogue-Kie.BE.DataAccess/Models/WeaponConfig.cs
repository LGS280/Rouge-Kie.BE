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

        public float FireRate { get; set; }
        public int ManaCost { get; set; }
        public int BulletsPerShot { get; set; }
        public float SpreadAngle { get; set; }

        public int BulletId { get; set; }

        [ForeignKey("BulletId")]
        public BulletConfig BulletConfig { get; set; }
    }
}
