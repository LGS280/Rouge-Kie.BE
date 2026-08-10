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

        /// <summary>
        /// Mua vật phẩm trong Cửa hàng bằng Gem hoặc Vàng
        /// </summary>
        [HttpPost("buy/{id}")]
        [Authorize]
        public async Task<IActionResult> BuyItem(int id)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Không tìm thấy thông tin tài khoản hợp lệ.");
            }

            var result = await _shopItemService.BuyItemAsync(userId, id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
