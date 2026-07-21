using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemsController : ControllerBase
    {
        private readonly IShopItemService _shopItemService;

        public ShopItemsController(IShopItemService shopItemService)
        {
            _shopItemService = shopItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _shopItemService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _shopItemService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Create([FromBody] CreateShopItemRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _shopItemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ShopItemId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShopItemRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
 
            var result = await _shopItemService.UpdateAsync(id, request);
            if (result == null) return NotFound();
 
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _shopItemService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
