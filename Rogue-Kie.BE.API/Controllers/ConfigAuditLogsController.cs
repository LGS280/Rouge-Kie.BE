using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rogue_Kie.BE.Business.Services.ConfigAuditLogs;
using Rogue_Kie.BE.Contracts.ConfigAuditLogs;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Controllers
{
    /// <summary>
    /// Controller quản lý và tra cứu nhật ký kiểm toán thay đổi cấu hình game (ConfigAuditLogs)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Developer")]
    public class ConfigAuditLogsController : ControllerBase
    {
        private readonly IConfigAuditLogService _auditLogService;

        public ConfigAuditLogsController(IConfigAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Lấy danh sách lịch sử kiểm toán có lọc và phân trang
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetConfigAuditLogsQuery query)
        {
            var result = await _auditLogService.GetAuditLogsAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Xem chi tiết một bản ghi lịch sử kiểm toán theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _auditLogService.GetByIdAsync(id);
            if (result == null) return NotFound(new { success = false, message = "Không tìm thấy bản ghi kiểm toán." });
            return Ok(result);
        }

        /// <summary>
        /// Lấy toàn bộ lịch sử chỉnh sửa dữ liệu của một đợt bảo trì (Patch Notes / Changelog của đợt bảo trì)
        /// </summary>
        [HttpGet("by-maintenance/{maintenanceId}")]
        public async Task<IActionResult> GetByMaintenance(int maintenanceId)
        {
            var result = await _auditLogService.GetLogsByMaintenanceIdAsync(maintenanceId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy toàn bộ lịch sử chỉnh sửa của một thực thể cụ thể (ví dụ: vũ khí ID 5, quái vật ID 2)
        /// </summary>
        [HttpGet("by-record/{tableName}/{recordId}")]
        public async Task<IActionResult> GetByRecord(string tableName, int recordId)
        {
            var result = await _auditLogService.GetLogsByRecordAsync(tableName, recordId);
            return Ok(result);
        }
    }
}
