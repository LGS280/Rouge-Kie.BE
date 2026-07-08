using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IBulletService
    {
        Task<IEnumerable<BulletResponse>> GetAllBulletsAsync();
        Task<BulletResponse?> GetBulletByIdAsync(int id);
        Task<BulletResponse> CreateBulletAsync(CreateBulletRequest request);
        Task<BulletResponse?> UpdateBulletAsync(int id, UpdateBulletRequest request);
        Task<bool> DeleteBulletAsync(int id);
    }
}
