using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface ICosmeticService
    {
        Task<List<CosmeticResponse>> GetAllAsync();
        Task<CosmeticResponse?> GetByIdAsync(int id);
        Task<CosmeticResponse> CreateAsync(CreateCosmeticRequest request);
        Task<CosmeticResponse?> UpdateAsync(int id, UpdateCosmeticRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
