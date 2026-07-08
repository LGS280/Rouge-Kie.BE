namespace Rogue_Kie.BE.Contracts.Profile
{
    public class UpdateProfileRequest
    {
        public string? DisplayName { get; set; }
        public int StandardCurrency { get; set; }
        public int PremiumCurrency { get; set; }
        public int HighestWave { get; set; }
        public int TotalKills { get; set; }
    }
}
