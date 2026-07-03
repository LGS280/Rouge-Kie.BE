using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface ILevelService
    {
        Task<List<LevelResponse>> GetAllAsync();
        Task<LevelResponse?> GetByIdAsync(int id);
        Task<LevelResponse> CreateAsync(CreateLevelRequest request);
        Task<LevelResponse?> UpdateAsync(UpdateLevelRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
