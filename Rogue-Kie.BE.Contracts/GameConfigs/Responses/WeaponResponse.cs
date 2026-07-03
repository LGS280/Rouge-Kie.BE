namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class WeaponResponse
    {
        public int Id { get; set; }
        public string WeaponName { get; set; } = string.Empty;
        public float FireRate { get; set; }
        public int ManaCost { get; set; }
        public int BulletsPerShot { get; set; }
        public float SpreadAngle { get; set; }
        public int BulletId { get; set; }
    }
}
