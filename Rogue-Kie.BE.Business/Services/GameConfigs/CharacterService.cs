using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public class CharacterService : ICharacterService
    {
        private readonly AppDbContext _context;

        public CharacterService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CharacterResponse>> GetAllAsync()
        {
            return await _context.Characters
                .Select(c => new CharacterResponse
                {
                    CharacterId = c.CharacterId,
                    Name = c.Name,
                    Description = c.Description,
                    BaseHealth = c.BaseHealth,
                    BaseMana = c.BaseMana,
                    BaseArmor = c.BaseArmor,
                    PrefabName = c.PrefabName,
                    SkillSet = c.SkillSet,
                    UnlockPrice = c.UnlockPrice,
                    CurrencyType = c.CurrencyType
                })
                .ToListAsync();
        }

        public async Task<CharacterResponse?> GetByIdAsync(int id)
        {
            var c = await _context.Characters.FindAsync(id);
            if (c == null) return null;

            return new CharacterResponse
            {
                CharacterId = c.CharacterId,
                Name = c.Name,
                Description = c.Description,
                BaseHealth = c.BaseHealth,
                BaseMana = c.BaseMana,
                BaseArmor = c.BaseArmor,
                PrefabName = c.PrefabName,
                SkillSet = c.SkillSet,
                UnlockPrice = c.UnlockPrice,
                CurrencyType = c.CurrencyType
            };
        }

        public async Task<CharacterResponse> CreateAsync(CreateCharacterRequest request)
        {
            var entity = new Character
            {
                Name = request.Name,
                Description = request.Description,
                BaseHealth = request.BaseHealth,
                BaseMana = request.BaseMana,
                BaseArmor = request.BaseArmor,
                PrefabName = request.PrefabName,
                SkillSet = request.SkillSet,
                UnlockPrice = request.UnlockPrice,
                CurrencyType = request.CurrencyType
            };

            _context.Characters.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.CharacterId) ?? throw new System.Exception("Failed to retrieve created character");
        }

        public async Task<CharacterResponse?> UpdateAsync(int id, UpdateCharacterRequest request)
        {
            var entity = await _context.Characters.FindAsync(id);
            if (entity == null) return null;

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.BaseHealth = request.BaseHealth;
            entity.BaseMana = request.BaseMana;
            entity.BaseArmor = request.BaseArmor;
            entity.PrefabName = request.PrefabName;
            entity.SkillSet = request.SkillSet;
            entity.UnlockPrice = request.UnlockPrice;
            entity.CurrencyType = request.CurrencyType;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.CharacterId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Characters.FindAsync(id);
            if (entity == null) return false;

            _context.Characters.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlayerCharacterResponse>> GetMyCharactersAsync(int userId)
        {
            var profile = await _context.PlayerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return new List<PlayerCharacterResponse>
                {
                    new PlayerCharacterResponse
                    {
                        CharacterId = 1,
                        Name = "Rookie",
                        PrefabName = "Rookie",
                        Description = "Default tactical combat operative.",
                        IsUnlocked = true,
                        UnlockedAt = System.DateTime.UtcNow
                    }
                };
            }

            var allCharacters = await _context.Characters.ToListAsync();
            var unlockedRecords = await _context.PlayerCharacters
                .Where(pc => pc.ProfileId == profile.ProfileId && pc.IsUnlocked)
                .ToListAsync();

            var result = new List<PlayerCharacterResponse>();

            foreach (var c in allCharacters)
            {
                bool isDefault = c.PrefabName.Equals("Rookie", System.StringComparison.OrdinalIgnoreCase)
                              || c.Name.Equals("Rookie", System.StringComparison.OrdinalIgnoreCase)
                              || c.Name.Equals("Kie Warrior", System.StringComparison.OrdinalIgnoreCase);

                var playerRecord = unlockedRecords.FirstOrDefault(pc => pc.CharacterId == c.CharacterId);
                bool isUnlocked = isDefault || (playerRecord != null && playerRecord.IsUnlocked);

                result.Add(new PlayerCharacterResponse
                {
                    Id = playerRecord?.Id ?? 0,
                    ProfileId = profile.ProfileId,
                    CharacterId = c.CharacterId,
                    Name = c.Name,
                    PrefabName = c.PrefabName,
                    Description = c.Description,
                    IsUnlocked = isUnlocked,
                    UnlockedAt = playerRecord?.UnlockedAt ?? (isDefault ? System.DateTime.UtcNow : null)
                });
            }

            if (!result.Any(r => r.PrefabName.Equals("Rookie", System.StringComparison.OrdinalIgnoreCase)))
            {
                result.Insert(0, new PlayerCharacterResponse
                {
                    CharacterId = 1,
                    Name = "Rookie",
                    PrefabName = "Rookie",
                    Description = "Default tactical combat operative.",
                    IsUnlocked = true,
                    UnlockedAt = System.DateTime.UtcNow
                });
            }

            return result;
        }
    }
}
