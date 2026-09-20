using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.Maintenance;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;

namespace Rogue_Kie.BE.Business.Services.Maintenance
{
    /// <summary>
    /// Dịch vụ xử lý logic lịch bảo trì hệ thống máy chủ
    /// </summary>
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;

        public MaintenanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MaintenanceResponse>> GetAllAsync()
        {
            var now = DateTime.UtcNow;
            await AutoUpdateStatusesAsync(now);

            var list = await _context.MaintenanceConfigs
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();

            return list.Select(m => MapToResponse(m, now)).ToList();
        }

        public static readonly string[] AllowedStatuses = { "Scheduled", "Active", "Completed", "Cancelled" };

        public async Task<MaintenanceResponse?> GetByIdAsync(int id)
        {
            var now = DateTime.UtcNow;
            await AutoUpdateStatusesAsync(now);

            var m = await _context.MaintenanceConfigs.FindAsync(id);
            if (m == null) return null;

            return MapToResponse(m, now);
        }

        public async Task<MaintenanceResponse> CreateAsync(CreateMaintenanceRequest request)
        {
            var utcStart = EnsureUtc(request.StartTime);
            var utcEnd = EnsureUtc(request.EndTime);
            var now = DateTime.UtcNow;

            // Đơn bảo trì mới tạo cách thời gian hiện tại ít nhất 5 phút nên trạng thái khởi tạo luôn là Scheduled
            string initialStatus = "Scheduled";

            // Kiểm tra trùng lặp khung giờ nếu đợt bảo trì là Active hoặc Scheduled
            var isOverlapping = await _context.MaintenanceConfigs.AnyAsync(m =>
                (m.Status == "Active" || m.Status == "Scheduled") &&
                utcStart < m.EndTime &&
                utcEnd > m.StartTime
            );

            if (isOverlapping)
            {
                throw new InvalidOperationException("Khung giờ bảo trì này bị trùng lặp với một đợt bảo trì khác (Active hoặc Scheduled).");
            }

            var entity = new MaintenanceConfig
            {
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                StartTime = utcStart,
                EndTime = utcEnd,
                Status = initialStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _context.MaintenanceConfigs.Add(entity);
            await _context.SaveChangesAsync();

            return MapToResponse(entity, DateTime.UtcNow);
        }

        public async Task<MaintenanceResponse?> UpdateAsync(int id, UpdateMaintenanceRequest request)
        {
            var entity = await _context.MaintenanceConfigs.FindAsync(id);
            if (entity == null) return null;

            var utcStart = EnsureUtc(request.StartTime);
            var utcEnd = EnsureUtc(request.EndTime);
            var now = DateTime.UtcNow;

            // Ràng buộc StartTime khi Update:
            // Nếu đợt bảo trì này chưa bắt đầu (entity.StartTime > now), hoặc admin muốn sửa StartTime sang một mốc khác (utcStart != entity.StartTime):
            // thì StartTime mới KHÔNG ĐƯỢC ở trong quá khứ so với thời điểm hiện tại và PHẢI cách hiện tại ít nhất 5 phút.
            if (entity.StartTime > now || utcStart != entity.StartTime)
            {
                if (utcStart <= now)
                {
                    throw new ArgumentException("Thời gian bắt đầu bảo trì không được sửa về quá khứ so với thời điểm hiện tại.");
                }

                if (utcStart < now.AddMinutes(5))
                {
                    throw new ArgumentException("Thời gian bắt đầu bảo trì phải cách thời gian hiện tại ít nhất 5 phút.");
                }
            }

            // Xác định trạng thái mục tiêu
            string targetStatus;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                targetStatus = NormalizeStatus(request.Status);
            }
            else
            {
                if (entity.Status == "Cancelled")
                {
                    targetStatus = "Cancelled";
                }
                else if (utcStart > now)
                {
                    targetStatus = "Scheduled";
                }
                else if (now <= utcEnd)
                {
                    targetStatus = "Active";
                }
                else
                {
                    targetStatus = "Completed";
                }
            }

            // Kiểm tra trùng lặp khung giờ nếu trạng thái là Active hoặc Scheduled (bỏ qua chính bản ghi đang sửa)
            if (targetStatus == "Active" || targetStatus == "Scheduled")
            {
                var isOverlapping = await _context.MaintenanceConfigs.AnyAsync(m =>
                    m.Id != id &&
                    (m.Status == "Active" || m.Status == "Scheduled") &&
                    utcStart < m.EndTime &&
                    utcEnd > m.StartTime
                );

                if (isOverlapping)
                {
                    throw new InvalidOperationException("Khung giờ bảo trì này bị trùng lặp với một đợt bảo trì khác (Active hoặc Scheduled).");
                }
            }

            entity.Title = request.Title.Trim();
            entity.Message = request.Message.Trim();
            entity.StartTime = utcStart;
            entity.EndTime = utcEnd;
            entity.Status = targetStatus;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity, DateTime.UtcNow);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.MaintenanceConfigs.FindAsync(id);
            if (entity == null) return false;

            // Xóa mềm: Chuyển trạng thái sang Cancelled thay vì xóa cứng khỏi cơ sở dữ liệu
            entity.Status = "Cancelled";
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CurrentMaintenanceStatusResponse> GetCurrentMaintenanceStatusAsync()
        {
            var now = DateTime.UtcNow;
            await AutoUpdateStatusesAsync(now);

            // Tìm đợt bảo trì Active hoặc Scheduled có khung giờ bao trùm thời điểm hiện tại
            var activeMaintenance = await _context.MaintenanceConfigs
                .Where(m => (m.Status == "Active" || m.Status == "Scheduled") && m.StartTime <= now && m.EndTime >= now)
                .OrderByDescending(m => m.EndTime)
                .FirstOrDefaultAsync();

            if (activeMaintenance != null)
            {
                var remaining = (int)Math.Max(0, (activeMaintenance.EndTime - now).TotalMinutes);
                return new CurrentMaintenanceStatusResponse
                {
                    IsUnderMaintenance = true,
                    Title = activeMaintenance.Title,
                    Message = activeMaintenance.Message,
                    StartTime = ToVnOffset(activeMaintenance.StartTime),
                    EndTime = ToVnOffset(activeMaintenance.EndTime),
                    RemainingMinutes = remaining
                };
            }

            return new CurrentMaintenanceStatusResponse
            {
                IsUnderMaintenance = false,
                Title = string.Empty,
                Message = string.Empty,
                StartTime = null,
                EndTime = null,
                RemainingMinutes = null
            };
        }

        private async Task AutoUpdateStatusesAsync(DateTime now)
        {
            var candidates = await _context.MaintenanceConfigs
                .Where(m => m.Status == "Scheduled" || m.Status == "Active")
                .ToListAsync();

            bool hasChanges = false;
            foreach (var m in candidates)
            {
                if (m.Status == "Scheduled" && now >= m.StartTime && now <= m.EndTime)
                {
                    m.Status = "Active";
                    m.UpdatedAt = now;
                    hasChanges = true;
                }
                else if ((m.Status == "Active" || m.Status == "Scheduled") && now > m.EndTime)
                {
                    m.Status = "Completed";
                    m.UpdatedAt = now;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        private static string NormalizeStatus(string status)
        {
            var trimmed = status.Trim();
            foreach (var allowed in AllowedStatuses)
            {
                if (allowed.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    return allowed;
                }
            }
            throw new ArgumentException($"Trạng thái '{status}' không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedStatuses)}");
        }

        private static MaintenanceResponse MapToResponse(MaintenanceConfig m, DateTime now)
        {
            var isCurrent = (m.Status == "Active" || m.Status == "Scheduled") && now >= m.StartTime && now <= m.EndTime;
            
            // Trạng thái hiển thị theo thời gian thực:
            // - Đang trong khoảng [StartTime, EndTime] và chưa bị Cancelled -> Active
            // - Đã quá EndTime và chưa bị Cancelled -> Completed
            string displayStatus = m.Status;
            if (m.Status == "Scheduled" && now >= m.StartTime && now <= m.EndTime)
            {
                displayStatus = "Active";
            }
            else if ((m.Status == "Active" || m.Status == "Scheduled") && now > m.EndTime)
            {
                displayStatus = "Completed";
            }

            return new MaintenanceResponse
            {
                Id = m.Id,
                Title = m.Title,
                Message = m.Message,
                StartTime = ToVnOffset(m.StartTime),
                EndTime = ToVnOffset(m.EndTime),
                Status = displayStatus,
                CreatedAt = ToVnOffset(m.CreatedAt),
                UpdatedAt = m.UpdatedAt.HasValue ? ToVnOffset(m.UpdatedAt.Value) : null,
                IsCurrentlyActive = isCurrent
            };
        }

        public static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        public static DateTime EnsureUtc(DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt;
            }
            if (dt.Kind == DateTimeKind.Local)
            {
                return dt.ToUniversalTime();
            }
            // DateTimeKind.Unspecified: Người dùng gửi chuỗi thời gian không có offset từ Swagger/Client (ví dụ "2026-09-20T12:00:00").
            // Mặc định coi đây là giờ Việt Nam (UTC+7) -> Quy đổi về UTC (-7 giờ) để lưu vào PostgreSQL (timestamptz).
            return DateTime.SpecifyKind(dt - VnOffset, DateTimeKind.Utc);
        }

        public static DateTimeOffset ToVnOffset(DateTime utcDt)
        {
            var utcClean = EnsureUtc(utcDt);
            return new DateTimeOffset(utcClean).ToOffset(VnOffset);
        }
    }
}
