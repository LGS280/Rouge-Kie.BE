namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class WeaponResponse
    {
        public int Id { get; set; }
        public string WeaponName { get; set; } = string.Empty;
        public string PrefabName { get; set; } = string.Empty;
        public float FireRate { get; set; }
        public int ManaCost { get; set; }
        public int BulletsPerShot { get; set; }
        public float SpreadAngle { get; set; }
        
        public string ShootSound { get; set; } = string.Empty;
        public float ShootVolume { get; set; } = 1.0f;
        
        public float HandPositionX { get; set; } = 0f;
        public float HandPositionY { get; set; } = 0f;
        public float HandPositionZ { get; set; } = 0f;
        
        public float RecoilDistance { get; set; } = 0.15f;
        public float RecoilDuration { get; set; } = 0.05f;
        public float ReturnDuration { get; set; } = 0.1f;

        public string WeaponType { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;

        public int BulletId { get; set; }
    }
}
