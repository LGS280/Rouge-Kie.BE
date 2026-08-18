using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.GameConfigs;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller Quản Lý Cửa Hàng Vật Phẩm (Shop Items Module).
    /// Quản lý danh mục gói nạp Gem, vật phẩm mua bằng Vàng/Gem, quy đổi tiền tệ và giao dịch kho đồ người chơi.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemsController : ControllerBase
    {
        private readonly IShopItemService _shopItemService;

        /// <summary>
        /// Khởi tạo Controller với Dependency Injection IShopItemService
        /// </summary>
        public ShopItemsController(IShopItemService shopItemService)
        {
            _shopItemService = shopItemService;
        }

        /// <summary>
        /// Endpoint: GET /api/shopitems
        /// Lấy danh sách toàn bộ vật phẩm & gói nạp đang bán trong Cửa Hàng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _shopItemService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Endpoint: GET /api/shopitems/{id}
        /// Lấy chi tiết thông tin một vật phẩm theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _shopItemService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Endpoint: POST /api/shopitems (Dành cho Admin/Developer)
        /// Tạo mới vật phẩm/gói nạp bán trong Cửa hàng
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Create([FromBody] CreateShopItemRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _shopItemService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ShopItemId }, result);
        }

        /// <summary>
        /// Endpoint: PUT /api/shopitems/{id} (Dành cho Admin/Developer)
        /// Cập nhật giá bán, tên gọi, hình ảnh của vật phẩm trong Cửa hàng
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShopItemRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
 
            var result = await _shopItemService.UpdateAsync(id, request);
            if (result == null) return NotFound();
 
            return Ok(result);
        }

        /// <summary>
        /// Endpoint: DELETE /api/shopitems/{id} (Dành cho Admin/Developer)
        /// Xóa một vật phẩm khỏi danh mục Cửa hàng
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _shopItemService.DeleteAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Endpoint: POST /api/shopitems/buy/{id}
        /// Mua vật phẩm trong Cửa hàng bằng Gem hoặc Vàng (Quy đổi tiền tệ hoặc thêm vào Inventory người chơi).
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
