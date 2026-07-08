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
    public class LevelService : ILevelService
    {
        private readonly AppDbContext _context;

        public LevelService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LevelResponse>> GetAllAsync()
        {
            return await _context.LevelConfigs
                .Select(l => new LevelResponse
                {
                    Id = l.Id,
                    FloorNumber = l.FloorNumber,
                    MaxEnemiesToSpawn = l.MaxEnemiesToSpawn,
                    DifficultyMultiplier = l.DifficultyMultiplier
                })
                .ToListAsync();
        }

        public async Task<LevelResponse?> GetByIdAsync(int id)
        {
            var l = await _context.LevelConfigs.FindAsync(id);
            if (l == null) return null;

            return new LevelResponse
            {
                Id = l.Id,
                FloorNumber = l.FloorNumber,
                MaxEnemiesToSpawn = l.MaxEnemiesToSpawn,
                DifficultyMultiplier = l.DifficultyMultiplier
            };
        }

        public async Task<LevelResponse> CreateAsync(CreateLevelRequest request)
        {
            var entity = new LevelConfig
            {
                FloorNumber = request.FloorNumber,
                MaxEnemiesToSpawn = request.MaxEnemiesToSpawn,
                DifficultyMultiplier = request.DifficultyMultiplier
            };

            _context.LevelConfigs.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new System.Exception("Failed to retrieve created entity");
        }

        public async Task<LevelResponse?> UpdateAsync(int id, UpdateLevelRequest request)
        {
            var entity = await _context.LevelConfigs.FindAsync(id);
            if (entity == null) return null;

            entity.FloorNumber = request.FloorNumber;
            entity.MaxEnemiesToSpawn = request.MaxEnemiesToSpawn;
            entity.DifficultyMultiplier = request.DifficultyMultiplier;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.LevelConfigs.FindAsync(id);
            if (entity == null) return false;

            _context.LevelConfigs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
