using Rogue_Kie.BE.Contracts.GameConfigs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IPlayerWeaponService
    {
        Task<IEnumerable<PlayerWeaponResponse>> GetMyWeaponsAsync(int userId);
        Task<UnlockWeaponResponse?> UnlockWeaponAsync(int userId, int weaponConfigId);
        Task<bool> DevResetMyWeaponsAsync(int userId);
        Task SyncPaidWeaponsAsync(int profileId, int userId);
    }
}
