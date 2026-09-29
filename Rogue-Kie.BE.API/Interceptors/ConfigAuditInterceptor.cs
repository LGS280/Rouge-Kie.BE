using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rogue_Kie.BE.DataAccess.DBContext;
using Rogue_Kie.BE.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.API.Interceptors
{
    /// <summary>
    /// Interceptor tự động bắt các thay đổi trên các bảng Cấu hình game (Game Configs)
    /// và ghi nhận vào bảng ConfigAuditLogs trước khi SaveChanges hoàn tất.
    /// </summary>
    public class ConfigAuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Danh sách các kiểu entity cấu hình cần theo dõi lịch sử
        private static readonly HashSet<Type> ConfigEntityTypes = new()
        {
            typeof(WeaponConfig),
            typeof(EnemyConfig),
            typeof(BulletConfig),
            typeof(BuffConfig),
            typeof(LevelConfig),
            typeof(RoomConfig),
            typeof(RoomWaveConfig),
            typeof(WaveEnemyDetail),
            typeof(Character),
            typeof(CosmeticItem),
            typeof(ShopItem),
            typeof(MaintenanceConfig)
        };

        public ConfigAuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is AppDbContext context)
            {
                CaptureAuditLogs(context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is AppDbContext context)
            {
                CaptureAuditLogs(context);
            }

            return base.SavingChanges(eventData, result);
        }

        private void CaptureAuditLogs(AppDbContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userName = GetCurrentUserName(httpContext);
            var (maintenanceId, reason) = GetMaintenanceAndReason(context, httpContext);
            var now = DateTime.UtcNow;

            var auditLogs = new List<ConfigAuditLog>();

            var entries = context.ChangeTracker.Entries()
                .Where(e => ConfigEntityTypes.Contains(e.Entity.GetType()))
                .ToList();

            foreach (var entry in entries)
            {
                var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
                var recordIdProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
                var recordId = recordIdProp?.CurrentValue as int? ?? recordIdProp?.OriginalValue as int? ?? 0;

                if (entry.State == EntityState.Modified)
                {
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.Metadata.IsPrimaryKey()) continue;
                        if (prop.Metadata.Name == "UpdatedAt") continue;

                        if (prop.IsModified)
                        {
                            var oldVal = prop.OriginalValue?.ToString();
                            var newVal = prop.CurrentValue?.ToString();

                            if (oldVal != newVal)
                            {
                                auditLogs.Add(new ConfigAuditLog
                                {
                                    MaintenanceId = maintenanceId,
                                    TableName = tableName,
                                    RecordId = recordId,
                                    Action = "UPDATE",
                                    FieldName = prop.Metadata.Name,
                                    OldValue = oldVal,
                                    NewValue = newVal,
                                    ChangedBy = userName,
                                    ChangedAt = now,
                                    Reason = reason
                                });
                            }
                        }
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditLogs.Add(new ConfigAuditLog
                    {
                        MaintenanceId = maintenanceId,
                        TableName = tableName,
                        RecordId = recordId,
                        Action = "DELETE",
                        FieldName = null,
                        OldValue = null,
                        NewValue = null,
                        ChangedBy = userName,
                        ChangedAt = now,
                        Reason = reason
                    });
                }
                else if (entry.State == EntityState.Added)
                {
                    auditLogs.Add(new ConfigAuditLog
                    {
                        MaintenanceId = maintenanceId,
                        TableName = tableName,
                        RecordId = recordId,
                        Action = "CREATE",
                        FieldName = null,
                        OldValue = null,
                        NewValue = null,
                        ChangedBy = userName,
                        ChangedAt = now,
                        Reason = reason
                    });
                }
            }

            if (auditLogs.Count > 0)
            {
                context.ConfigAuditLogs.AddRange(auditLogs);
            }
        }

        private static string GetCurrentUserName(HttpContext? httpContext)
        {
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.FindFirst(ClaimTypes.Name)?.Value
                    ?? httpContext.User.FindFirst(ClaimTypes.Email)?.Value
                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? httpContext.User.Identity.Name
                    ?? "Admin";
            }

            return "Admin";
        }

        private static (int? MaintenanceId, string? Reason) GetMaintenanceAndReason(AppDbContext context, HttpContext? httpContext)
        {
            int? maintenanceId = null;
            string? reason = null;

            if (httpContext != null)
            {
                if (httpContext.Request.Headers.TryGetValue("X-Maintenance-Id", out var mVal) && int.TryParse(mVal, out var parsedId))
                {
                    maintenanceId = parsedId;
                }

                if (httpContext.Request.Headers.TryGetValue("X-Audit-Reason", out var rVal))
                {
                    try
                    {
                        reason = Uri.UnescapeDataString(rVal.ToString());
                    }
                    catch
                    {
                        reason = rVal.ToString();
                    }
                }
            }

            // Nếu không truyền trong header, tự động quét tìm đợt bảo trì đang Active hiện tại
            if (!maintenanceId.HasValue)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var activeMaint = context.MaintenanceConfigs
                        .AsNoTracking()
                        .Where(m => m.Status == "Active" && m.StartTime <= now && m.EndTime >= now)
                        .Select(m => new { m.Id, m.Title })
                        .FirstOrDefault();

                    if (activeMaint != null)
                    {
                        maintenanceId = activeMaint.Id;
                        if (string.IsNullOrWhiteSpace(reason))
                        {
                            reason = $"Tự động gán theo đợt bảo trì: {activeMaint.Title}";
                        }
                    }
                }
                catch
                {
                    // Tránh gây lỗi SaveChanges nếu query Active maintenance gặp ngoại lệ
                }
            }

            return (maintenanceId, reason);
        }
    }
}
