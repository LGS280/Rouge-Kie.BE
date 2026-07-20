using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class BulletConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BulletName { get; set; }

        public int Damage { get; set; }
        public float CritRate { get; set; }
        public float CritMultiplier { get; set; } = 1.5f;
        public float FlightSpeed { get; set; }
        public int PiercingCount { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrefabName { get; set; }
    }
}
