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
                    PrefabName = w.PrefabName,
                    FireRate = w.FireRate,
                    ManaCost = w.ManaCost,
                    BulletsPerShot = w.BulletsPerShot,
                    SpreadAngle = w.SpreadAngle,
                    ShootSound = w.ShootSound,
                    ShootVolume = w.ShootVolume,
                    HandPositionX = w.HandPositionX,
                    HandPositionY = w.HandPositionY,
                    HandPositionZ = w.HandPositionZ,
                    RecoilDistance = w.RecoilDistance,
                    RecoilDuration = w.RecoilDuration,
                    ReturnDuration = w.ReturnDuration,
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
                PrefabName = w.PrefabName,
                FireRate = w.FireRate,
                ManaCost = w.ManaCost,
                BulletsPerShot = w.BulletsPerShot,
                SpreadAngle = w.SpreadAngle,
                ShootSound = w.ShootSound,
                ShootVolume = w.ShootVolume,
                HandPositionX = w.HandPositionX,
                HandPositionY = w.HandPositionY,
                HandPositionZ = w.HandPositionZ,
                RecoilDistance = w.RecoilDistance,
                RecoilDuration = w.RecoilDuration,
                ReturnDuration = w.ReturnDuration,
                BulletId = w.BulletId
            };
        }

        public async Task<WeaponResponse> CreateAsync(CreateWeaponRequest request)
        {
            var entity = new WeaponConfig
            {
                WeaponName = request.WeaponName,
                PrefabName = request.PrefabName,
                FireRate = request.FireRate,
                ManaCost = request.ManaCost,
                BulletsPerShot = request.BulletsPerShot,
                SpreadAngle = request.SpreadAngle,
                ShootSound = request.ShootSound,
                ShootVolume = request.ShootVolume,
                HandPositionX = request.HandPositionX,
                HandPositionY = request.HandPositionY,
                HandPositionZ = request.HandPositionZ,
                RecoilDistance = request.RecoilDistance,
                RecoilDuration = request.RecoilDuration,
                ReturnDuration = request.ReturnDuration,
                BulletId = request.BulletId
            };

            _context.WeaponConfigs.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new System.Exception("Failed to retrieve created entity");
        }

        public async Task<WeaponResponse?> UpdateAsync(int id, UpdateWeaponRequest request)
        {
            var entity = await _context.WeaponConfigs.FindAsync(id);
            if (entity == null) return null;

            entity.WeaponName = request.WeaponName;
            entity.PrefabName = request.PrefabName;
            entity.FireRate = request.FireRate;
            entity.ManaCost = request.ManaCost;
            entity.BulletsPerShot = request.BulletsPerShot;
            entity.SpreadAngle = request.SpreadAngle;
            entity.ShootSound = request.ShootSound;
            entity.ShootVolume = request.ShootVolume;
            entity.HandPositionX = request.HandPositionX;
            entity.HandPositionY = request.HandPositionY;
            entity.HandPositionZ = request.HandPositionZ;
            entity.RecoilDistance = request.RecoilDistance;
            entity.RecoilDuration = request.RecoilDuration;
            entity.ReturnDuration = request.ReturnDuration;
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
