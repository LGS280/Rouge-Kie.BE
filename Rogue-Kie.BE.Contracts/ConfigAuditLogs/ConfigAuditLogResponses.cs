using System;

namespace Rogue_Kie.BE.Contracts.ConfigAuditLogs
{
    /// <summary>
    /// DTO thông tin chi tiết một bản ghi lịch sử kiểm toán cấu hình game
    /// </summary>
    public class ConfigAuditLogResponse
    {
        public int Id { get; set; }

        /// <summary>
        /// ID đợt bảo trì (nếu có)
        /// </summary>
        public int? MaintenanceId { get; set; }

        /// <summary>
        /// Tiêu đề đợt bảo trì (được join từ MaintenanceConfig)
        /// </summary>
        public string? MaintenanceTitle { get; set; }

        /// <summary>
        /// Tên bảng dữ liệu (vd: WeaponConfigs, EnemyConfigs,...)
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// ID dòng dữ liệu bị chỉnh sửa
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// Hành động: CREATE, UPDATE, DELETE
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Tên thuộc tính bị sửa
        /// </summary>
        public string? FieldName { get; set; }

        /// <summary>
        /// Giá trị cũ
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// Giá trị mới
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Người thực hiện (Username/Email)
        /// </summary>
        public string ChangedBy { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm chỉnh sửa UTC
        /// </summary>
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// Thời điểm chỉnh sửa quy đổi sang giờ Việt Nam (UTC+7)
        /// </summary>
        public string ChangedAtVn => ChangedAt.AddHours(7).ToString("dd/MM/yyyy HH:mm:ss");

        /// <summary>
        /// Ghi chú lý do chỉnh sửa
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO phản hồi danh sách phân trang
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T>
    {
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public System.Collections.Generic.List<T> Items { get; set; } = new();
    }
}
