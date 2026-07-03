using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IEnemyService
    {
        Task<List<EnemyResponse>> GetAllAsync();
        Task<EnemyResponse?> GetByIdAsync(int id);
        Task<EnemyResponse> CreateAsync(CreateEnemyRequest request);
        Task<EnemyResponse?> UpdateAsync(UpdateEnemyRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
