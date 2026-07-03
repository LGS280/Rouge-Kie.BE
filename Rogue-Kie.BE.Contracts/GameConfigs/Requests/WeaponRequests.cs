using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateWeaponRequest
    {
        [Required]
        [MaxLength(50)]
        public string WeaponName { get; set; } = string.Empty;

        [Required]
        public int Damage { get; set; }

        [Required]
        public float FireRate { get; set; }
    }

    public class UpdateWeaponRequest : CreateWeaponRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
