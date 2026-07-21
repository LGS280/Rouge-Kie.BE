namespace Rogue_Kie.BE.Contracts.Leaderboard
{
    public class LeaderboardResponse
    {
        public int Rank { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public int HighestWave { get; set; }
        public int TotalKills { get; set; }
        public int TotalRuns { get; set; }
    }
}
