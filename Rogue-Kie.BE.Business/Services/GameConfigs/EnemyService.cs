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
    public class EnemyService : IEnemyService
    {
        private readonly AppDbContext _context;

        public EnemyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EnemyResponse>> GetAllAsync()
        {
            return await _context.EnemyConfigs
                .Select(e => new EnemyResponse
                {
                    Id = e.Id,
                    EnemyName = e.EnemyName,
                    BaseHealth = e.BaseHealth,
                    BaseDamage = e.BaseDamage,
                    MoveSpeed = e.MoveSpeed,
                    AttackSpeed = e.AttackSpeed,
                    PrefabName = e.PrefabName
                })
                .ToListAsync();
        }

        public async Task<EnemyResponse?> GetByIdAsync(int id)
        {
            var e = await _context.EnemyConfigs.FindAsync(id);
            if (e == null) return null;

            return new EnemyResponse
            {
                Id = e.Id,
                EnemyName = e.EnemyName,
                BaseHealth = e.BaseHealth,
                BaseDamage = e.BaseDamage,
                MoveSpeed = e.MoveSpeed,
                AttackSpeed = e.AttackSpeed,
                PrefabName = e.PrefabName
            };
        }

        public async Task<EnemyResponse> CreateAsync(CreateEnemyRequest request)
        {
            var entity = new EnemyConfig
            {
                EnemyName = request.EnemyName,
                BaseHealth = request.BaseHealth,
                BaseDamage = request.BaseDamage,
                MoveSpeed = request.MoveSpeed,
                AttackSpeed = request.AttackSpeed,
                PrefabName = request.PrefabName
            };

            _context.EnemyConfigs.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new System.Exception("Failed to retrieve created entity");
        }

        public async Task<EnemyResponse?> UpdateAsync(int id, UpdateEnemyRequest request)
        {
            var entity = await _context.EnemyConfigs.FindAsync(id);
            if (entity == null) return null;

            entity.EnemyName = request.EnemyName;
            entity.BaseHealth = request.BaseHealth;
            entity.BaseDamage = request.BaseDamage;
            entity.MoveSpeed = request.MoveSpeed;
            entity.AttackSpeed = request.AttackSpeed;
            entity.PrefabName = request.PrefabName;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.EnemyConfigs.FindAsync(id);
            if (entity == null) return false;

            _context.EnemyConfigs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
