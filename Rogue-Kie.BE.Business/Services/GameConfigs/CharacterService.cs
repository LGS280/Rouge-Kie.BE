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
                    BaseDamage = c.BaseDamage,
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
                BaseDamage = c.BaseDamage,
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
                BaseDamage = request.BaseDamage,
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
            entity.BaseDamage = request.BaseDamage;
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
    }
}
