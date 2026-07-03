using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameConfigsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GameConfigsController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DÀNH CHO UNITY CLIENT TẢI DỮ LIỆU LÚC ĐẦU
        // ==========================================
        [HttpGet("sync")]
        public async Task<IActionResult> SyncAllConfigs()
        {
            var enemies = await _context.EnemyConfigs.ToListAsync();
            var weapons = await _context.WeaponConfigs.ToListAsync();
            var levels = await _context.LevelConfigs.ToListAsync();
            var buffs = await _context.BuffConfigs.ToListAsync();

            return Ok(new
            {
                enemies,
                weapons,
                levels,
                buffs
            });
        }

        // ==========================================
        // DÀNH CHO WEB ADMIN (CRUD Enemy)
        // ==========================================
        [HttpGet("enemies")]
        public async Task<IActionResult> GetEnemies() => Ok(await _context.EnemyConfigs.ToListAsync());

        [HttpPost("enemies")]
        public async Task<IActionResult> CreateEnemy([FromBody] EnemyConfig config)
        {
            _context.EnemyConfigs.Add(config);
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPut("enemies/{id}")]
        public async Task<IActionResult> UpdateEnemy(int id, [FromBody] EnemyConfig config)
        {
            if (id != config.Id) return BadRequest();
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpDelete("enemies/{id}")]
        public async Task<IActionResult> DeleteEnemy(int id)
        {
            var config = await _context.EnemyConfigs.FindAsync(id);
            if (config == null) return NotFound();
            _context.EnemyConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ==========================================
        // DÀNH CHO WEB ADMIN (CRUD Weapon)
        // ==========================================
        [HttpGet("weapons")]
        public async Task<IActionResult> GetWeapons() => Ok(await _context.WeaponConfigs.ToListAsync());

        [HttpPost("weapons")]
        public async Task<IActionResult> CreateWeapon([FromBody] WeaponConfig config)
        {
            _context.WeaponConfigs.Add(config);
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPut("weapons/{id}")]
        public async Task<IActionResult> UpdateWeapon(int id, [FromBody] WeaponConfig config)
        {
            if (id != config.Id) return BadRequest();
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpDelete("weapons/{id}")]
        public async Task<IActionResult> DeleteWeapon(int id)
        {
            var config = await _context.WeaponConfigs.FindAsync(id);
            if (config == null) return NotFound();
            _context.WeaponConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ==========================================
        // DÀNH CHO WEB ADMIN (CRUD Level)
        // ==========================================
        [HttpGet("levels")]
        public async Task<IActionResult> GetLevels() => Ok(await _context.LevelConfigs.ToListAsync());

        [HttpPost("levels")]
        public async Task<IActionResult> CreateLevel([FromBody] LevelConfig config)
        {
            _context.LevelConfigs.Add(config);
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPut("levels/{id}")]
        public async Task<IActionResult> UpdateLevel(int id, [FromBody] LevelConfig config)
        {
            if (id != config.Id) return BadRequest();
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpDelete("levels/{id}")]
        public async Task<IActionResult> DeleteLevel(int id)
        {
            var config = await _context.LevelConfigs.FindAsync(id);
            if (config == null) return NotFound();
            _context.LevelConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ==========================================
        // DÀNH CHO WEB ADMIN (CRUD Buff)
        // ==========================================
        [HttpGet("buffs")]
        public async Task<IActionResult> GetBuffs() => Ok(await _context.BuffConfigs.ToListAsync());

        [HttpPost("buffs")]
        public async Task<IActionResult> CreateBuff([FromBody] BuffConfig config)
        {
            _context.BuffConfigs.Add(config);
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPut("buffs/{id}")]
        public async Task<IActionResult> UpdateBuff(int id, [FromBody] BuffConfig config)
        {
            if (id != config.Id) return BadRequest();
            _context.Entry(config).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpDelete("buffs/{id}")]
        public async Task<IActionResult> DeleteBuff(int id)
        {
            var config = await _context.BuffConfigs.FindAsync(id);
            if (config == null) return NotFound();
            _context.BuffConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
