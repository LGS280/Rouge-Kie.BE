using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    /// <summary>
    /// Thực thể cấu hình lịch bảo trì hệ thống máy chủ
    /// </summary>
    public class MaintenanceConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Tiêu đề bảo trì (Ví dụ: "Thông Báo Bảo Trì Định Kỳ")
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung chi tiết thông báo bảo trì gửi đến người chơi
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian bắt đầu bảo trì (UTC)
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc dự kiến (UTC)
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Trạng thái đợt bảo trì: Active (Đang kích hoạt/Lên lịch), Completed (Đã hoàn tất), Cancelled (Đã hủy)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Thời điểm tạo bản ghi (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời điểm cập nhật bản ghi lần cuối (UTC)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
