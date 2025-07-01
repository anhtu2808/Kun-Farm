using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Interfaces;
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

        public Task<ApiResponse<bool>> BuyItem(int playerId, BuyItemRequest request)
        {
           var playerSlot = _playerRegularShopSlotRepository.PlayerRegularShopSlot(playerId, request.SlotId).Result;

            return Task.Run(async () =>
            {
                var slot = await _regularShopSlotRepository.GetById(request.SlotId);
                if (slot == null || !slot.CanBuy)
                {
                    return new ApiResponse<bool>
                    {
                        Data = false,
                        Message = "Item cannot be bought."
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

                playerSlot.CurrentStock += request.Quantity;
                
                // Update the player's shop slot
                await _playerRegularShopSlotRepository.UpdateAsync(playerSlot);

                return new ApiResponse<bool>
                {
                    Data = true,
                    Message = "Item bought successfully."
                };
            });
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
    }
}
