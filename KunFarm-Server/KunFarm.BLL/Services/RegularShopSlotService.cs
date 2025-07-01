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

        public RegularShopSlotService(IRegularShopSlotRepository regularShopSlotRepository, IItemRepository itemRepository)
        {
            _regularShopSlotRepository = regularShopSlotRepository;
            _itemRepository = itemRepository;
        }

        public Task<ApiResponse<List<ShopItemResponse>>> GetShopItem()
        {
            return Task.Run(async () =>
           {
               var slots = await _regularShopSlotRepository.GetAllSlot();
               var shopItems = new List<ShopItemResponse>();

               foreach (var slot in slots)
               {
                   var item = await _itemRepository.GetItemById(slot.ItemId);
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
                           SellPrice = slot.SellPrice,
                           CanSell = slot.CanSell,
                           ShowInShop = slot.ShowInShop,
                           StockLimit = slot.StockLimit,
                           CurrentStock = slot.CurrentStock
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
