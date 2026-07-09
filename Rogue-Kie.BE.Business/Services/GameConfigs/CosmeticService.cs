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
    public class CosmeticService : ICosmeticService
    {
        private readonly AppDbContext _context;

        public CosmeticService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CosmeticResponse>> GetAllAsync()
        {
            return await _context.CosmeticItems
                .Select(c => new CosmeticResponse
                {
                    CosmeticId = c.CosmeticId,
                    Name = c.Name,
                    Type = c.Type,
                    Rarity = c.Rarity,
                    Price = c.Price,
                    CurrencyType = c.CurrencyType
                })
                .ToListAsync();
        }

        public async Task<CosmeticResponse?> GetByIdAsync(int id)
        {
            var c = await _context.CosmeticItems.FindAsync(id);
            if (c == null) return null;

            return new CosmeticResponse
            {
                CosmeticId = c.CosmeticId,
                Name = c.Name,
                Type = c.Type,
                Rarity = c.Rarity,
                Price = c.Price,
                CurrencyType = c.CurrencyType
            };
        }

        public async Task<CosmeticResponse> CreateAsync(CreateCosmeticRequest request)
        {
            var entity = new CosmeticItem
            {
                Name = request.Name,
                Type = request.Type,
                Rarity = request.Rarity,
                Price = request.Price,
                CurrencyType = request.CurrencyType
            };

            _context.CosmeticItems.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.CosmeticId) ?? throw new System.Exception("Failed to retrieve created cosmetic");
        }

        public async Task<CosmeticResponse?> UpdateAsync(int id, UpdateCosmeticRequest request)
        {
            var entity = await _context.CosmeticItems.FindAsync(id);
            if (entity == null) return null;

            entity.Name = request.Name;
            entity.Type = request.Type;
            entity.Rarity = request.Rarity;
            entity.Price = request.Price;
            entity.CurrencyType = request.CurrencyType;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.CosmeticId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.CosmeticItems.FindAsync(id);
            if (entity == null) return false;

            _context.CosmeticItems.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
