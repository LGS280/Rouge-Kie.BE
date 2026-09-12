using System;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class PlayerWeaponResponse
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int WeaponConfigId { get; set; }
        public string WeaponName { get; set; } = string.Empty;
        public string PrefabName { get; set; } = string.Empty;
        public string WeaponType { get; set; } = "Rifle";
        public string Rarity { get; set; } = "Common";
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }

    public class UnlockWeaponResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int WeaponConfigId { get; set; }
        public string WeaponName { get; set; } = string.Empty;
        public string PrefabName { get; set; } = string.Empty;
    }
}
