using Microsoft.EntityFrameworkCore;
using Rogue_Kie.BE.Contracts.ConfigAuditLogs;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.ConfigAuditLogs
{
    /// <summary>
    /// Lớp thực thi dịch vụ tra cứu và ghi nhận lịch sử kiểm toán cấu hình game
    /// </summary>
    public class ConfigAuditLogService : IConfigAuditLogService
    {
        private readonly AppDbContext _context;

        public ConfigAuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ConfigAuditLogResponse>> GetAuditLogsAsync(GetConfigAuditLogsQuery query)
        {
            var q = _context.ConfigAuditLogs
                .Include(x => x.Maintenance)
                .AsNoTracking()
                .AsQueryable();

            if (query.MaintenanceId.HasValue)
            {
                q = q.Where(x => x.MaintenanceId == query.MaintenanceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.TableName))
            {
                var tableLower = query.TableName.Trim().ToLower();
                q = q.Where(x => x.TableName.ToLower().Contains(tableLower));
            }

            if (query.RecordId.HasValue)
            {
                q = q.Where(x => x.RecordId == query.RecordId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Action))
            {
                var actionUpper = query.Action.Trim().ToUpper();
                q = q.Where(x => x.Action.ToUpper() == actionUpper);
            }

            if (!string.IsNullOrWhiteSpace(query.ChangedBy))
            {
                var userLower = query.ChangedBy.Trim().ToLower();
                q = q.Where(x => x.ChangedBy.ToLower().Contains(userLower));
            }

            if (query.FromDate.HasValue)
            {
                q = q.Where(x => x.ChangedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                q = q.Where(x => x.ChangedAt <= query.ToDate.Value);
            }

            var totalItems = await q.CountAsync();

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 100 ? 100 : query.PageSize);

            var items = await q
                .OrderByDescending(x => x.ChangedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => MapToResponse(x))
                .ToListAsync();

            return new PagedResult<ConfigAuditLogResponse>
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<ConfigAuditLogResponse?> GetByIdAsync(int id)
        {
            var log = await _context.ConfigAuditLogs
                .Include(x => x.Maintenance)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return log == null ? null : MapToResponse(log);
        }

        public async Task<List<ConfigAuditLogResponse>> GetLogsByMaintenanceIdAsync(int maintenanceId)
        {
            return await _context.ConfigAuditLogs
                .Include(x => x.Maintenance)
                .AsNoTracking()
                .Where(x => x.MaintenanceId == maintenanceId)
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => MapToResponse(x))
                .ToListAsync();
        }

        public async Task<List<ConfigAuditLogResponse>> GetLogsByRecordAsync(string tableName, int recordId)
        {
            var tableLower = tableName.Trim().ToLower();
            return await _context.ConfigAuditLogs
                .Include(x => x.Maintenance)
                .AsNoTracking()
                .Where(x => x.TableName.ToLower() == tableLower && x.RecordId == recordId)
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => MapToResponse(x))
                .ToListAsync();
        }

        public async Task LogChangeAsync(ConfigAuditLog log)
        {
            _context.ConfigAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private static ConfigAuditLogResponse MapToResponse(ConfigAuditLog x)
        {
            return new ConfigAuditLogResponse
            {
                Id = x.Id,
                MaintenanceId = x.MaintenanceId,
                MaintenanceTitle = x.Maintenance != null ? x.Maintenance.Title : null,
                TableName = x.TableName,
                RecordId = x.RecordId,
                Action = x.Action,
                FieldName = x.FieldName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ChangedBy = x.ChangedBy,
                ChangedAt = x.ChangedAt,
                Reason = x.Reason
            };
        }
    }
}
