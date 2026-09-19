using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.Maintenance;
using Rogue_Kie.BE.Contracts.Maintenance;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller quản lý lịch bảo trì máy chủ và cung cấp trạng thái bảo trì cho Game Client
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        /// <summary>
        /// API công khai cho Game Client và Web kiểm tra trạng thái bảo trì hiện tại của máy chủ
        /// </summary>
        [HttpGet("current")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentStatus()
        {
            var status = await _maintenanceService.GetCurrentMaintenanceStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách lịch bảo trì (Dành cho Quản trị viên)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _maintenanceService.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một đợt bảo trì theo ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _maintenanceService.GetByIdAsync(id);
            if (item == null) return NotFound(new { message = $"Không tìm thấy đợt bảo trì có ID {id}" });
            return Ok(item);
        }

        /// <summary>
        /// Tạo mới một đợt bảo trì hệ thống
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (request.EndTime <= request.StartTime)
            {
                return BadRequest(new { message = "Thời gian kết thúc phải lớn hơn thời gian bắt đầu." });
            }

            var result = await _maintenanceService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Cập nhật thông tin đợt bảo trì
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaintenanceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (request.EndTime <= request.StartTime)
            {
                return BadRequest(new { message = "Thời gian kết thúc phải lớn hơn thời gian bắt đầu." });
            }

            var result = await _maintenanceService.UpdateAsync(id, request);
            if (result == null) return NotFound(new { message = $"Không tìm thấy đợt bảo trì có ID {id}" });

            return Ok(result);
        }

        /// <summary>
        /// Xóa một đợt bảo trì
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _maintenanceService.DeleteAsync(id);
            if (!success) return NotFound(new { message = $"Không tìm thấy đợt bảo trì có ID {id}" });

            return NoContent();
        }
    }
}
