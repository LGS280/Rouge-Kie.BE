using System.Collections.Generic;

namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class GameConfigsSyncResponse
    {
        public List<EnemyResponse> Enemies { get; set; } = new List<EnemyResponse>();
        public List<WeaponResponse> Weapons { get; set; } = new List<WeaponResponse>();
        public List<LevelResponse> Levels { get; set; } = new List<LevelResponse>();
        public List<BuffResponse> Buffs { get; set; } = new List<BuffResponse>();
    }
}
