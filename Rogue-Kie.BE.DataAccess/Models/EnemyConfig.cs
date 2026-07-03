using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.DataAccess.Models
{
    public class EnemyConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnemyName { get; set; }

        public int BaseHealth { get; set; }
        public int BaseDamage { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackSpeed { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrefabName { get; set; }
    }
}
