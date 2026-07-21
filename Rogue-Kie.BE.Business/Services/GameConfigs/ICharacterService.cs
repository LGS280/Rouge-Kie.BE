using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface ICharacterService
    {
        Task<List<CharacterResponse>> GetAllAsync();
        Task<CharacterResponse?> GetByIdAsync(int id);
        Task<CharacterResponse> CreateAsync(CreateCharacterRequest request);
        Task<CharacterResponse?> UpdateAsync(int id, UpdateCharacterRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
