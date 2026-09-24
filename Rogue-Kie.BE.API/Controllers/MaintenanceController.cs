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
        /// API công khai lấy thông tin đợt bảo trì sắp tới gần nhất (phục vụ thông báo lịch)
        /// </summary>
        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUpcoming()
        {
            var upcoming = await _maintenanceService.GetUpcomingMaintenanceAsync();
            if (upcoming == null)
            {
                return Ok(new { hasUpcoming = false, message = "Không có lịch bảo trì nào sắp diễn ra." });
            }
            return Ok(new { hasUpcoming = true, data = upcoming });
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

            var utcStart = MaintenanceService.EnsureUtc(request.StartTime);
            var utcEnd = MaintenanceService.EnsureUtc(request.EndTime);
            var now = DateTime.UtcNow;

            // 1. Kiểm tra thời gian trong quá khứ khi tạo mới
            if (utcStart <= now)
            {
                return BadRequest(new { message = "Thời gian bắt đầu bảo trì không được ở trong quá khứ." });
            }

            if (utcEnd <= now)
            {
                return BadRequest(new { message = "Thời gian kết thúc bảo trì không được ở trong quá khứ." });
            }

            // 2. Kiểm tra tính hợp lệ của khoảng thời gian
            if (utcEnd <= utcStart)
            {
                return BadRequest(new { message = "Thời gian kết thúc phải lớn hơn thời gian bắt đầu." });
            }

            // 3. Quy định: Chỉ được tạo bảo trì cách thời gian hiện tại ít nhất 5 phút
            if (utcStart < now.AddMinutes(5))
            {
                return BadRequest(new { message = "Thời gian bắt đầu bảo trì phải cách thời gian hiện tại ít nhất 5 phút." });
            }

            try
            {
                var result = await _maintenanceService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (System.InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin đợt bảo trì
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaintenanceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!string.IsNullOrWhiteSpace(request.Status) && !MaintenanceService.AllowedStatuses.Contains(request.Status.Trim(), System.StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = $"Trạng thái không hợp lệ. Chỉ chấp nhận: {string.Join(", ", MaintenanceService.AllowedStatuses)}" });
            }

            var utcStart = MaintenanceService.EnsureUtc(request.StartTime);
            var utcEnd = MaintenanceService.EnsureUtc(request.EndTime);

            if (utcEnd <= utcStart)
            {
                return BadRequest(new { message = "Thời gian kết thúc phải lớn hơn thời gian bắt đầu." });
            }

            string statusNorm = request.Status?.Trim() ?? "Active";
            if ((statusNorm.Equals("Active", System.StringComparison.OrdinalIgnoreCase) || statusNorm.Equals("Scheduled", System.StringComparison.OrdinalIgnoreCase)) && utcEnd <= System.DateTime.UtcNow)
            {
                return BadRequest(new { message = "Thời gian kết thúc của đợt bảo trì (Active/Scheduled) phải ở trong tương lai." });
            }

            try
            {
                var result = await _maintenanceService.UpdateAsync(id, request);
                if (result == null) return NotFound(new { message = $"Không tìm thấy đợt bảo trì có ID {id}" });

                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa một đợt bảo trì (Xóa mềm: Chuyển trạng thái sang Cancelled)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _maintenanceService.DeleteAsync(id);
            if (!success) return NotFound(new { message = $"Không tìm thấy đợt bảo trì có ID {id}" });

            return Ok(new { message = $"Đã hủy bỏ đợt bảo trì ID {id} thành công (chuyển trạng thái sang Cancelled)." });
        }
    }
}
