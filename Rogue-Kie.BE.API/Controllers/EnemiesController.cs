using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Developer")]
    public class EnemiesController : ControllerBase
    {
        private readonly IEnemyService _enemyService;

        public EnemiesController(IEnemyService enemyService)
        {
            _enemyService = enemyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _enemyService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _enemyService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnemyRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _enemyService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEnemyRequest request)
        {
            if (id != request.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _enemyService.UpdateAsync(request);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _enemyService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
