using System;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class PlayerCharacterResponse
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int CharacterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PrefabName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }
}
