using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CosmeticsController : ControllerBase
    {
        private readonly ICosmeticService _cosmeticService;

        public CosmeticsController(ICosmeticService cosmeticService)
        {
            _cosmeticService = cosmeticService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _cosmeticService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _cosmeticService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Create([FromBody] CreateCosmeticRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _cosmeticService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.CosmeticId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCosmeticRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
 
            var result = await _cosmeticService.UpdateAsync(id, request);
            if (result == null) return NotFound();
 
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _cosmeticService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
