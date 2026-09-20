using System;
using System.ComponentModel.DataAnnotations;

namespace Rogue_Kie.BE.Contracts.Maintenance
{
    /// <summary>
    /// Yêu cầu tạo mới một lịch bảo trì hệ thống
    /// </summary>
    public class CreateMaintenanceRequest
    {
        [Required(ErrorMessage = "Tiêu đề bảo trì là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tiêu đề không được vượt quá 150 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung chi tiết là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Nội dung chi tiết không được vượt quá 1000 ký tự")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// Yêu cầu cập nhật lịch bảo trì hệ thống
    /// </summary>
    public class UpdateMaintenanceRequest
    {
        [Required(ErrorMessage = "Tiêu đề bảo trì là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tiêu đề không được vượt quá 150 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung chi tiết là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Nội dung chi tiết không được vượt quá 1000 ký tự")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        public DateTime EndTime { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Scheduled, Active, Completed, Cancelled
    }
}
