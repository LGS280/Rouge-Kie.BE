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
    /// Thông tin đợt bảo trì sắp tới gần nhất (phục vụ thông báo trước cho người chơi)
    /// </summary>
    public class UpcomingMaintenanceInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int HoursUntilStart { get; set; }
        public int MinutesUntilStart { get; set; }
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

        /// <summary>
        /// Có đợt bảo trì sắp tới trong khung giờ báo trước (<= 48h) hay không
        /// </summary>
        public bool HasUpcomingMaintenance { get; set; }

        /// <summary>
        /// Chi tiết đợt bảo trì sắp tới gần nhất
        /// </summary>
        public UpcomingMaintenanceInfo? UpcomingMaintenance { get; set; }
    }
}
