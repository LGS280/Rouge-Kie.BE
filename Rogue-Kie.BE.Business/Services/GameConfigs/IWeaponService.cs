using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IWeaponService
    {
        Task<List<WeaponResponse>> GetAllAsync();
        Task<WeaponResponse?> GetByIdAsync(int id);
        Task<WeaponResponse> CreateAsync(CreateWeaponRequest request);
        Task<WeaponResponse?> UpdateAsync(UpdateWeaponRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
