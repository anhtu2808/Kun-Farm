using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.Interfaces
{
    public interface IOnlineShopService
    {
        Task<ApiResponse<List<SellItemResponse>>> GetRandomSoldItem(int playerId);
        Task<ApiResponse<bool>> BuyItem(int playerId, List<int> request);
        Task<ApiResponse<SellItemResponse>> SellItem(int playerId, SellItemRequest request);

        // Task 3: New methods
        Task<ApiResponse<List<SellItemResponse>>> GetSoldItemsByPlayer(int playerId);
        Task<ApiResponse<bool>> ClaimMoney(int playerId, List<int> soldItemIds);
    }
}
