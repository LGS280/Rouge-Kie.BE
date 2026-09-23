using System;

namespace Rogue_Kie.BE.Contracts.ConfigAuditLogs
{
    /// <summary>
    /// DTO tiêu chí tìm kiếm và phân trang lịch sử kiểm toán
    /// </summary>
    public class GetConfigAuditLogsQuery
    {
        /// <summary>
        /// Lọc theo ID đợt bảo trì
        /// </summary>
        public int? MaintenanceId { get; set; }

        /// <summary>
        /// Lọc theo tên bảng (vd: WeaponConfigs, EnemyConfigs,...)
        /// </summary>
        public string? TableName { get; set; }

        /// <summary>
        /// Lọc theo ID của bản ghi cấu hình cụ thể
        /// </summary>
        public int? RecordId { get; set; }

        /// <summary>
        /// Lọc theo hành động: CREATE, UPDATE, DELETE
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Lọc theo người thực hiện
        /// </summary>
        public string? ChangedBy { get; set; }

        /// <summary>
        /// Từ ngày (UTC)
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Đến ngày (UTC)
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Trang số (mặc định: 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Số phần tử trên một trang (mặc định: 20, tối đa: 100)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
