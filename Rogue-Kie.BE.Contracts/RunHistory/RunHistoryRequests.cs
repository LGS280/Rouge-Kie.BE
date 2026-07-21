namespace Rogue_Kie.BE.Contracts.RunHistory
{
    public class CreateRunHistoryRequest
    {
        public int CharacterId { get; set; }
        public int WavesSurvived { get; set; }
        public int EnemiesKilled { get; set; }
        public int DamageDealt { get; set; }
        public int CurrencyEarned { get; set; }
        public int DurationSeconds { get; set; }
    }
}
