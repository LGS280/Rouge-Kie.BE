using System;

namespace Rogue_Kie.BE.Contracts.RunHistory
{
    public class RunHistoryResponse
    {
        public int RunId { get; set; }
        public int UserId { get; set; }
        public int CharacterId { get; set; }
        public string CharacterName { get; set; } = string.Empty;
        public int WavesSurvived { get; set; }
        public int EnemiesKilled { get; set; }
        public int DamageDealt { get; set; }
        public int CurrencyEarned { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}
