using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public class BulletService : IBulletService
    {
        private readonly AppDbContext _context;

        public BulletService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BulletResponse>> GetAllBulletsAsync()
        {
            var bullets = await _context.BulletConfigs.ToListAsync();
            return bullets.Select(b => MapToResponse(b));
        }

        public async Task<BulletResponse?> GetBulletByIdAsync(int id)
        {
            var bullet = await _context.BulletConfigs.FindAsync(id);
            if (bullet == null) return null;

            return MapToResponse(bullet);
        }

        public async Task<BulletResponse> CreateBulletAsync(CreateBulletRequest request)
        {
            var bullet = new BulletConfig
            {
                BulletName = request.BulletName,
                Damage = request.Damage,
                CritRate = request.CritRate,
                CritMultiplier = request.CritMultiplier,
                FlightSpeed = request.FlightSpeed,
                PiercingCount = request.PiercingCount,
                PrefabName = request.PrefabName
            };

            _context.BulletConfigs.Add(bullet);
            await _context.SaveChangesAsync();

            return MapToResponse(bullet);
        }

        public async Task<BulletResponse?> UpdateBulletAsync(int id, UpdateBulletRequest request)
        {
            var bullet = await _context.BulletConfigs.FindAsync(id);
            if (bullet == null) return null;

            bullet.BulletName = request.BulletName;
            bullet.Damage = request.Damage;
            bullet.CritRate = request.CritRate;
            bullet.CritMultiplier = request.CritMultiplier;
            bullet.FlightSpeed = request.FlightSpeed;
            bullet.PiercingCount = request.PiercingCount;
            bullet.PrefabName = request.PrefabName;

            _context.BulletConfigs.Update(bullet);
            await _context.SaveChangesAsync();

            return MapToResponse(bullet);
        }

        public async Task<bool> DeleteBulletAsync(int id)
        {
            var bullet = await _context.BulletConfigs.FindAsync(id);
            if (bullet == null) return false;

            _context.BulletConfigs.Remove(bullet);
            await _context.SaveChangesAsync();
            return true;
        }

        private static BulletResponse MapToResponse(BulletConfig config)
        {
            return new BulletResponse
            {
                Id = config.Id,
                BulletName = config.BulletName,
                Damage = config.Damage,
                CritRate = config.CritRate,
                CritMultiplier = config.CritMultiplier,
                FlightSpeed = config.FlightSpeed,
                PiercingCount = config.PiercingCount,
                PrefabName = config.PrefabName
            };
        }
    }
}
