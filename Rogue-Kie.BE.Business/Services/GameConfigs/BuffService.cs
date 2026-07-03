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
    public class BuffService : IBuffService
    {
        private readonly AppDbContext _context;

        public BuffService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BuffResponse>> GetAllAsync()
        {
            return await _context.BuffConfigs
                .Select(b => new BuffResponse
                {
                    Id = b.Id,
                    BuffName = b.BuffName,
                    Description = b.Description,
                    BuffType = b.BuffType,
                    Value = b.Value,
                    Rarity = b.Rarity
                })
                .ToListAsync();
        }

        public async Task<BuffResponse?> GetByIdAsync(int id)
        {
            var b = await _context.BuffConfigs.FindAsync(id);
            if (b == null) return null;

            return new BuffResponse
            {
                Id = b.Id,
                BuffName = b.BuffName,
                Description = b.Description,
                BuffType = b.BuffType,
                Value = b.Value,
                Rarity = b.Rarity
            };
        }

        public async Task<BuffResponse> CreateAsync(CreateBuffRequest request)
        {
            var entity = new BuffConfig
            {
                BuffName = request.BuffName,
                Description = request.Description,
                BuffType = request.BuffType,
                Value = request.Value,
                Rarity = request.Rarity
            };

            _context.BuffConfigs.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new System.Exception("Failed to retrieve created entity");
        }

        public async Task<BuffResponse?> UpdateAsync(UpdateBuffRequest request)
        {
            var entity = await _context.BuffConfigs.FindAsync(request.Id);
            if (entity == null) return null;

            entity.BuffName = request.BuffName;
            entity.Description = request.Description;
            entity.BuffType = request.BuffType;
            entity.Value = request.Value;
            entity.Rarity = request.Rarity;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.BuffConfigs.FindAsync(id);
            if (entity == null) return false;

            _context.BuffConfigs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
