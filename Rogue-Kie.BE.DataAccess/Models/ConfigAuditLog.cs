using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rogue_Kie.BE.DataAccess.Models
{
    /// <summary>
    /// Thực thể lưu trữ lịch sử kiểm toán (Audit Log) các thay đổi cấu hình game,
    /// liên kết trực tiếp với đợt bảo trì hệ thống (MaintenanceConfig).
    /// </summary>
    public class ConfigAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID đợt bảo trì tương ứng (nếu thay đổi diễn ra trong đợt bảo trì).
        /// Null nếu là chỉnh sửa thông thường hoặc hotfix ngoài giờ bảo trì.
        /// </summary>
        public int? MaintenanceId { get; set; }

        /// <summary>
        /// Thực thể đợt bảo trì liên kết
        /// </summary>
        [ForeignKey(nameof(MaintenanceId))]
        public virtual MaintenanceConfig? Maintenance { get; set; }

        /// <summary>
        /// Tên bảng dữ liệu được thay đổi (ví dụ: WeaponConfigs, EnemyConfigs, BulletConfigs,...)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Khóa chính (ID) của dòng dữ liệu bị chỉnh sửa (ví dụ: Weapon ID 5, Enemy ID 2)
        /// </summary>
        [Required]
        public int RecordId { get; set; }

        /// <summary>
        /// Hành động thực hiện: CREATE, UPDATE, DELETE
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = "UPDATE";

        /// <summary>
        /// Tên cột / thuộc tính bị thay đổi (ví dụ: Damage, Health, FireRate).
        /// Có thể null trong trường hợp DELETE hoặc CREATE nguyên bản ghi.
        /// </summary>
        [MaxLength(100)]
        public string? FieldName { get; set; }

        /// <summary>
        /// Giá trị cũ trước khi thay đổi (chuyển đổi sang chuỗi text)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// Giá trị mới sau khi thay đổi (chuyển đổi sang chuỗi text)
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Danh tính tài khoản thực hiện sửa đổi (Username hoặc Email từ JWT Token)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ChangedBy { get; set; } = "System";

        /// <summary>
        /// Thời điểm chỉnh sửa (UTC)
        /// </summary>
        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lý do chỉnh sửa / ghi chú (ví dụ: "Nerf máu boss theo phản hồi người chơi")
        /// </summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
