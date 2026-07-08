using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BulletsController : ControllerBase
    {
        private readonly IBulletService _bulletService;

        public BulletsController(IBulletService bulletService)
        {
            _bulletService = bulletService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BulletResponse>>> GetAll()
        {
            var result = await _bulletService.GetAllBulletsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BulletResponse>> GetById(int id)
        {
            var result = await _bulletService.GetBulletByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<ActionResult<BulletResponse>> Create(CreateBulletRequest request)
        {
            var result = await _bulletService.CreateBulletAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<ActionResult<BulletResponse>> Update(int id, UpdateBulletRequest request)
        {
            var result = await _bulletService.UpdateBulletAsync(id, request);
            if (result == null) return NotFound();
 
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _bulletService.DeleteBulletAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}


