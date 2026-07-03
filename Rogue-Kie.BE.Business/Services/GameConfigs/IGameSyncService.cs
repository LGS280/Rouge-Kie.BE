using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IGameSyncService
    {
        Task<GameConfigsSyncResponse> GetSyncDataAsync();
    }
}
