using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Requests
{
    public class CreateEnemyRequest
    {
        [Required]
        [MaxLength(50)]
        public string EnemyName { get; set; } = string.Empty;

        [Required]
        public int BaseHealth { get; set; }

        [Required]
        public int BaseDamage { get; set; }

        [Required]
        public float MoveSpeed { get; set; }

        [Required]
        public float AttackSpeed { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrefabName { get; set; } = string.Empty;
    }

    public class UpdateEnemyRequest : CreateEnemyRequest
    {
        [Required]
        public int Id { get; set; }
    }
}
