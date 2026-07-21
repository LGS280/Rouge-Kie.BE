using System.Collections.Generic;
using System.Threading.Tasks;
using Rogue_Kie.BE.Contracts.GameConfigs.Requests;
using Rogue_Kie.BE.Contracts.GameConfigs.Responses;

namespace Rogue_Kie.BE.Business.Services.GameConfigs
{
    public interface IShopItemService
    {
        Task<List<ShopItemResponse>> GetAllAsync();
        Task<ShopItemResponse?> GetByIdAsync(int id);
        Task<ShopItemResponse> CreateAsync(CreateShopItemRequest request);
        Task<ShopItemResponse?> UpdateAsync(int id, UpdateShopItemRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
