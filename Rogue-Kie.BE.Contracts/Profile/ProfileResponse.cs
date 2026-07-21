using System;

namespace Rogue_Kie.BE.Contracts.Profile
{
    public class PlayerProfileResponse
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string? DisplayName { get; set; }
        public int StandardCurrency { get; set; }
        public int PremiumCurrency { get; set; }
        public int TotalRuns { get; set; }
        public int HighestWave { get; set; }
        public int TotalKills { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
