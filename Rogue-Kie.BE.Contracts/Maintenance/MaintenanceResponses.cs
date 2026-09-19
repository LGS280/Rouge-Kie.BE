using System;

namespace Rogue_Kie.BE.Contracts.Maintenance
{
    /// <summary>
    /// Thông tin chi tiết một đợt bảo trì hệ thống
    /// </summary>
    public class MaintenanceResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }

    /// <summary>
    /// Trạng thái bảo trì hiện tại phục vụ Game Client và Web kiểm tra máy chủ
    /// </summary>
    public class CurrentMaintenanceStatusResponse
    {
        public bool IsUnderMaintenance { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? RemainingMinutes { get; set; }
    }
}
