using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.Maintenance;

namespace Rogue_Kie.BE.Business.Services.Maintenance
{
    /// <summary>
    /// Giao diện dịch vụ quản lý lịch bảo trì hệ thống
    /// </summary>
    public interface IMaintenanceService
    {
        Task<List<MaintenanceResponse>> GetAllAsync();
        Task<MaintenanceResponse?> GetByIdAsync(int id);
        Task<MaintenanceResponse> CreateAsync(CreateMaintenanceRequest request);
        Task<MaintenanceResponse?> UpdateAsync(int id, UpdateMaintenanceRequest request);
        Task<bool> DeleteAsync(int id);
        Task<CurrentMaintenanceStatusResponse> GetCurrentMaintenanceStatusAsync();
        Task<UpcomingMaintenanceInfo?> GetUpcomingMaintenanceAsync();
    }
}
