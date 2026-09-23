using Rogue_Kie.BE.Contracts.ConfigAuditLogs;
using Rogue_Kie.BE.DataAccess.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.ConfigAuditLogs
{
    /// <summary>
    /// Giao diện dịch vụ quản lý và tra cứu lịch sử kiểm toán cấu hình game
    /// </summary>
    public interface IConfigAuditLogService
    {
        /// <summary>
        /// Truy vấn danh sách lịch sử kiểm toán theo bộ lọc và phân trang
        /// </summary>
        Task<PagedResult<ConfigAuditLogResponse>> GetAuditLogsAsync(GetConfigAuditLogsQuery query);

        /// <summary>
        /// Lấy chi tiết một bản ghi lịch sử kiểm toán theo ID
        /// </summary>
        Task<ConfigAuditLogResponse?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy toàn bộ lịch sử chỉnh sửa dữ liệu thuộc về một đợt bảo trì cụ thể (Patch Notes / Changelog của đợt bảo trì)
        /// </summary>
        Task<List<ConfigAuditLogResponse>> GetLogsByMaintenanceIdAsync(int maintenanceId);

        /// <summary>
        /// Lấy toàn bộ lịch sử thay đổi của một thực thể cấu hình cụ thể (ví dụ: WeaponId = 5)
        /// </summary>
        Task<List<ConfigAuditLogResponse>> GetLogsByRecordAsync(string tableName, int recordId);

        /// <summary>
        /// Ghi nhận trực tiếp một bản ghi kiểm toán
        /// </summary>
        Task LogChangeAsync(ConfigAuditLog log);
    }
}
