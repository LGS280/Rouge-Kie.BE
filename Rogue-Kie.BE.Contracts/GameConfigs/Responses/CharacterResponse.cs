namespace Rogue_Kie.BE.Contracts.GameConfigs.Responses
{
    public class CharacterResponse
    {
        public int CharacterId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int BaseHealth { get; set; }
        public int BaseMana { get; set; }
        public int BaseArmor { get; set; }
        public string PrefabName { get; set; } = string.Empty;
        public string? SkillSet { get; set; }
        public int UnlockPrice { get; set; }
        public string? CurrencyType { get; set; }
    }
}
