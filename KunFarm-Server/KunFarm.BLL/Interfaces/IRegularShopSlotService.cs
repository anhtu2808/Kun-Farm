using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.Interfaces
{
    public interface IRegularShopSlotService
    {
        Task<ApiResponse<List<ShopItemResponse>>> GetShopItem(int playerId);

        Task<ApiResponse<bool>> BuyItem(int playerId, List<BuyItemRequest> request);
    }
}
