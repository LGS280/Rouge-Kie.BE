using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;
using Rogue_Kie.BE.DataAccess.DBContext;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public class GameSyncService : IGameSyncService
    {
        private readonly AppDbContext _context;

        public GameSyncService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GameConfigsSyncResponse> GetSyncDataAsync()
        {
            var enemies = await _context.EnemyConfigs
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

            var weapons = await _context.WeaponConfigs
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

            var levels = await _context.LevelConfigs
                .Select(l => new LevelResponse
                {
                    Id = l.Id,
                    StageId = l.StageId,
                    FloorNumber = l.FloorNumber,
                    DifficultyMultiplier = l.DifficultyMultiplier
                })
                .ToListAsync();

            var buffs = await _context.BuffConfigs
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

            var bullets = await _context.BulletConfigs
                .Select(b => new BulletResponse
                {
                    Id = b.Id,
                    BulletName = b.BulletName,
                    Damage = b.Damage,
                    CritRate = b.CritRate,
                    FlightSpeed = b.FlightSpeed,
                    PiercingCount = b.PiercingCount,
                    PrefabName = b.PrefabName
                })
                .ToListAsync();

            return new GameConfigsSyncResponse
            {
                Enemies = enemies,
                Weapons = weapons,
                Levels = levels,
                Buffs = buffs,
                Bullets = bullets
            };
        }
    }
}
