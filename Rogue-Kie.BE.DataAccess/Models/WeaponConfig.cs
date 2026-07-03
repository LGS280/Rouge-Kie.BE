using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class WeaponConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string WeaponName { get; set; }

        public int Damage { get; set; }
        public float FireRate { get; set; }
        public int AmmoCapacity { get; set; }
        public float ReloadTime { get; set; }
    }
}
