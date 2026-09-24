using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rogue_Kie.BE.DataAccess.DBContext;

namespace Rogue_Kie.BE.API.Workers
{
    /// <summary>
    /// Background Service tự động quét và cập nhật trạng thái các đợt bảo trì:
    /// - Scheduled -> Active: Khi thời gian hiện tại chạm đến StartTime
    /// - Active/Scheduled -> Completed: Khi thời gian hiện tại đã vượt qua EndTime
    /// </summary>
    public class MaintenanceStatusWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaintenanceStatusWorker> _logger;

        public MaintenanceStatusWorker(IServiceProvider serviceProvider, ILogger<MaintenanceStatusWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MaintenanceStatusWorker đã khởi động.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;
                    var candidates = await context.MaintenanceConfigs
                        .Where(m => m.Status == "Scheduled" || m.Status == "Active")
                        .ToListAsync(stoppingToken);

                    bool hasChanges = false;
                    foreach (var m in candidates)
                    {
                        if (m.Status == "Scheduled" && now >= m.StartTime && now <= m.EndTime)
                        {
                            m.Status = "Active";
                            m.UpdatedAt = now;
                            hasChanges = true;
                            _logger.LogInformation("Lịch bảo trì #{Id} ({Title}) tự động chuyển sang trạng thái Active.", m.Id, m.Title);
                        }
                        else if ((m.Status == "Active" || m.Status == "Scheduled") && now > m.EndTime)
                        {
                            m.Status = "Completed";
                            m.UpdatedAt = now;
                            hasChanges = true;
                            _logger.LogInformation("Lịch bảo trì #{Id} ({Title}) tự động chuyển sang trạng thái Completed.", m.Id, m.Title);
                        }
                    }

                    if (hasChanges)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong quá trình cập nhật trạng thái bảo trì tự động.");
                }

                // Chạy kiểm tra định kỳ mỗi 15 giây
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
