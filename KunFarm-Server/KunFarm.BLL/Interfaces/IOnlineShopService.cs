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
        Task<ApiResponse<SellItemResponse>> SellItem(int playerId, SellItemRequest request);
        Task<ApiResponse<bool>> BuyItem(int playerId, List<int> request);

        Task<ApiResponse<List<SellItemResponse>>> GetRandomSoldItem(int playerId);
    }
}
