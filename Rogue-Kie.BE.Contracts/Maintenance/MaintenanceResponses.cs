using System;

namespace Rogue_Kie.BE.Contracts.Maintenance
{
    /// <summary>
    /// Thông tin chi tiết một đợt bảo trì hệ thống (Múi giờ Việt Nam UTC+7)
    /// </summary>
    public class MaintenanceResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }

    /// <summary>
    /// Trạng thái bảo trì hiện tại phục vụ Game Client và Web kiểm tra máy chủ (Múi giờ Việt Nam UTC+7)
    /// </summary>
    public class CurrentMaintenanceStatusResponse
    {
        public bool IsUnderMaintenance { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int? RemainingMinutes { get; set; }
    }
}
