using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IBuffService
    {
        Task<List<BuffResponse>> GetAllAsync();
        Task<BuffResponse?> GetByIdAsync(int id);
        Task<BuffResponse> CreateAsync(CreateBuffRequest request);
        Task<BuffResponse?> UpdateAsync(int id, UpdateBuffRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
