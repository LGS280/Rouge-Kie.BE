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
    public class WeaponService : IWeaponService
    {
        private readonly AppDbContext _context;

        public WeaponService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WeaponResponse>> GetAllAsync()
        {
            return await _context.WeaponConfigs
                .Select(w => new WeaponResponse
                {
                    Id = w.Id,
                    WeaponName = w.WeaponName,
                    FireRate = w.FireRate,
                    ManaCost = w.ManaCost,
                    BulletsPerShot = w.BulletsPerShot,
                    SpreadAngle = w.SpreadAngle,
                    BulletId = w.BulletId
                })
                .ToListAsync();
        }

        public async Task<WeaponResponse?> GetByIdAsync(int id)
        {
            var w = await _context.WeaponConfigs.FindAsync(id);
            if (w == null) return null;

            return new WeaponResponse
            {
                Id = w.Id,
                WeaponName = w.WeaponName,
                FireRate = w.FireRate,
                ManaCost = w.ManaCost,
                BulletsPerShot = w.BulletsPerShot,
                SpreadAngle = w.SpreadAngle,
                BulletId = w.BulletId
            };
        }

        public async Task<WeaponResponse> CreateAsync(CreateWeaponRequest request)
        {
            var entity = new WeaponConfig
            {
                WeaponName = request.WeaponName,
                FireRate = request.FireRate,
                ManaCost = request.ManaCost,
                BulletsPerShot = request.BulletsPerShot,
                SpreadAngle = request.SpreadAngle,
                BulletId = request.BulletId
            };

            _context.WeaponConfigs.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new System.Exception("Failed to retrieve created entity");
        }

        public async Task<WeaponResponse?> UpdateAsync(UpdateWeaponRequest request)
        {
            var entity = await _context.WeaponConfigs.FindAsync(request.Id);
            if (entity == null) return null;

            entity.WeaponName = request.WeaponName;
            entity.FireRate = request.FireRate;
            entity.ManaCost = request.ManaCost;
            entity.BulletsPerShot = request.BulletsPerShot;
            entity.SpreadAngle = request.SpreadAngle;
            entity.BulletId = request.BulletId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.WeaponConfigs.FindAsync(id);
            if (entity == null) return false;

            _context.WeaponConfigs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
