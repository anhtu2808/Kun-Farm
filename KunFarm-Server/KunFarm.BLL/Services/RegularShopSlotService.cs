using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using KunFarm.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.Services
{
    public class RegularShopSlotService : IRegularShopSlotService
    {
        private readonly IRegularShopSlotRepository _regularShopSlotRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPlayerRegularShopSlotRepository _playerRegularShopSlotRepository;

        public RegularShopSlotService(IRegularShopSlotRepository regularShopSlotRepository, IItemRepository itemRepository, IPlayerRegularShopSlotRepository playerRegularShopSlotRepository)
        {
            _regularShopSlotRepository = regularShopSlotRepository;
            _itemRepository = itemRepository;
            _playerRegularShopSlotRepository = playerRegularShopSlotRepository;
        }

        public async Task<ApiResponse<bool>> BuyItem(int playerId, List<BuyItemRequest> requests)
        {
            foreach (var request in requests)
            {
                var playerSlot = await _playerRegularShopSlotRepository.PlayerRegularShopSlot(playerId, request.SlotId);
                var slot = await _regularShopSlotRepository.GetById(request.SlotId);

                if (slot == null || !slot.CanBuy)
                {
                    return new ApiResponse<bool>
                    {
                        Data = false,
                        Message = $"Item with SlotId {request.SlotId} cannot be bought."
                    };
                }
                var item = await _itemRepository.GetItemById(slot.ItemId);
                if (item == null)
                {
                    return new ApiResponse<bool>
                    {
                        Data = false,
                        Message = "Item not found."
                    };
                }

                int totalCount = playerSlot.CurrentStock.Value + request.Quantity;

                if (totalCount >= slot.StockLimit)
                {
                    playerSlot.CurrentStock = slot.StockLimit;
                    slot.CanBuy = false;
                }
                else
                {
                    playerSlot.CurrentStock = totalCount;
                }

                // Update the player's shop slot
                await _playerRegularShopSlotRepository.UpdateAsync(playerSlot);
                await _regularShopSlotRepository.UpdateAsync(slot);
            }

            return new ApiResponse<bool>
            {
                Code = 200,
                Data = true,
                Message = "Tất cả item đã được mua thành công."
            };

        }

        public Task<ApiResponse<List<ShopItemResponse>>> GetShopItem(int player)
        {
            return Task.Run(async () =>
           {
               var slots = await _regularShopSlotRepository.GetAllSlot();
               var shopItems = new List<ShopItemResponse>();

               foreach (var slot in slots)
               {
                   var item = await _itemRepository.GetItemById(slot.ItemId);
                   var playerSlot = _playerRegularShopSlotRepository.PlayerRegularShopSlot(player, slot.Id).Result;
                   if (item != null)
                   {
                       shopItems.Add(new ShopItemResponse
                       {
                           SlotId = slot.Id,
                           CollectableType = item.CollectableType,
                           ItemName = item.ItemName,
                           BuyPrice = slot.BuyPrice,
                           CanBuy = slot.CanBuy,
                           Icon = item.Icon,
                           StockLimit = slot.StockLimit,
                           CurrentStock = playerSlot.CurrentStock,
                       });
                   }
               }

               return new ApiResponse<List<ShopItemResponse>>
               {
                   Data = shopItems,
                   Message = "Shop items retrieved successfully."
               };
           });
        }
        public async Task CreatePlayerSlot(int playerId)
        {
           
            var regularShopSlots = await _regularShopSlotRepository.GetAllSlot();
            foreach (var slot in regularShopSlots)
            {
                var playerSlot = new PlayerRegularShopSlot
                {
                    PlayerStateId = playerId,
                    RegularShopSlotId = slot.Id,
                    CurrentStock = 0,
                };
                await _playerRegularShopSlotRepository.CreateAsync(playerSlot);
            }
        }

    }
}
