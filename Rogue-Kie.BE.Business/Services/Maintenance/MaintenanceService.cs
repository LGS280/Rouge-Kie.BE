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
            var list = await _context.MaintenanceConfigs
                .OrderByDescending(m => m.StartTime)
                .ToListAsync();

            return list.Select(m => MapToResponse(m, now)).ToList();
        }

        public async Task<MaintenanceResponse?> GetByIdAsync(int id)
        {
            var m = await _context.MaintenanceConfigs.FindAsync(id);
            if (m == null) return null;

            return MapToResponse(m, DateTime.UtcNow);
        }

        public async Task<MaintenanceResponse> CreateAsync(CreateMaintenanceRequest request)
        {
            var entity = new MaintenanceConfig
            {
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                StartTime = EnsureUtc(request.StartTime),
                EndTime = EnsureUtc(request.EndTime),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
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

            entity.Title = request.Title.Trim();
            entity.Message = request.Message.Trim();
            entity.StartTime = EnsureUtc(request.StartTime);
            entity.EndTime = EnsureUtc(request.EndTime);
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                entity.Status = request.Status.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity, DateTime.UtcNow);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.MaintenanceConfigs.FindAsync(id);
            if (entity == null) return false;

            _context.MaintenanceConfigs.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CurrentMaintenanceStatusResponse> GetCurrentMaintenanceStatusAsync()
        {
            var now = DateTime.UtcNow;

            // Tìm đợt bảo trì Active có khung giờ bao trùm thời điểm hiện tại
            var activeMaintenance = await _context.MaintenanceConfigs
                .Where(m => m.Status == "Active" && m.StartTime <= now && m.EndTime >= now)
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
                    StartTime = activeMaintenance.StartTime,
                    EndTime = activeMaintenance.EndTime,
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

        private static MaintenanceResponse MapToResponse(MaintenanceConfig m, DateTime now)
        {
            var isCurrent = m.Status == "Active" && now >= m.StartTime && now <= m.EndTime;
            return new MaintenanceResponse
            {
                Id = m.Id,
                Title = m.Title,
                Message = m.Message,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                Status = m.Status,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                IsCurrentlyActive = isCurrent
            };
        }

        private static DateTime EnsureUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }
    }
}
