namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class EnemyResponse
    {
        public int Id { get; set; }
        public string EnemyName { get; set; } = string.Empty;
        public int BaseHealth { get; set; }
        public int BaseDamage { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackSpeed { get; set; }
        public string PrefabName { get; set; } = string.Empty;
    }
}
