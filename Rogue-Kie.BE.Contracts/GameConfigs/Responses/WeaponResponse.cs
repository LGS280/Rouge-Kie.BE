namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class WeaponResponse
    {
        public int Id { get; set; }
        public string WeaponName { get; set; } = string.Empty;
        public int Damage { get; set; }
        public float FireRate { get; set; }
    }
}
