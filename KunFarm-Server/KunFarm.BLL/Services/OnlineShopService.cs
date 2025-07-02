using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.Services
{
    public class OnlineShopService : IOnlineShopService
    {
        private readonly IOnlineShopRepository _onlineShopRepository;
        private readonly IItemRepository _itemRepository;

        public OnlineShopService(IOnlineShopRepository onlineShopRepository, IItemRepository itemRepository)
        {
            _onlineShopRepository = onlineShopRepository;
            _itemRepository = itemRepository;
        }

        public async Task<ApiResponse<List<SellItemResponse>>> GetRandomSoldItem(int playerId)
        {
            List<SellItemResponse> responses = new List<SellItemResponse>();
            var shop = await _onlineShopRepository.GetAllWithoutSellerId(playerId);
            foreach (OnlineShopSlot slot in shop)
            {
                Item? item = await _itemRepository.GetItemById(slot.ItemId);
                SellItemResponse response = new()
                {
                    CollectableType = item.CollectableType,
                    Price = slot.BuyPrice,
                    Quantity = slot.Quantiy,
                    Icon = item.Icon,
                    Id = slot.Id,
                    CanBuy = slot.CanBuy
                };
                responses.Add(response);
            }
            return new ApiResponse<List<SellItemResponse>>
            {
                Code = 200,
                Data = responses,
                Message = "Random sold items retrieved successfully."
            };
        }

        public async Task<ApiResponse<bool>> BuyItem(int playerId, List<int> request)
        {
            foreach(var item in request)
            {
                var slot = await _onlineShopRepository.GetByIdAsync(item);
                slot.BuyerId = playerId;
                slot.CanBuy = false;
                await _onlineShopRepository.UpdateAsync(slot);
            }

            return new ApiResponse<bool>
            {
                Code = 200,
                Data = true,
                Message = "Items bought successfully."
            };
        }

        public async Task<ApiResponse<SellItemResponse>> SellItem(int playerId, SellItemRequest request)
        {
            Item? item = await _itemRepository.GetItemByType(request.CollectableType);

            var slot = await _onlineShopRepository.AddAsync
                (
                new()
                {
                    CanBuy = true,
                    BuyPrice = request.Price,
                    ItemId = item.Id,
                    Quantiy = request.Quantity,
                    SellerId = playerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });

            SellItemResponse response = new()
            {
                CollectableType = item.CollectableType,
                Price = slot.BuyPrice,
                Quantity = slot.Quantiy,
                Icon = item.Icon,
                Id = slot.Id,
                CanBuy = slot.CanBuy
            };

            return new()
            {
                Code = 200,
                Data = response,
                Message = "Item sold successfully."
            };
        }
    }
}
