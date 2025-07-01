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
    public class InventorySlotService : IInventorySlotService
    {

        private readonly IInventorySlotRepository _inventorySlotRepository;
        private readonly IItemRepository _itemRepository;

        public InventorySlotService(IInventorySlotRepository inventorySlotRepository, IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
            _inventorySlotRepository = inventorySlotRepository;
        }

        public async Task<ApiResponse<List<InventorySlotResponse>>> GetInventorySlots(int playerId)
        {
            var inventorySlots = await _inventorySlotRepository.GetAllByPlayer(playerId);
            var inventorySlotResponses = new List<InventorySlotResponse>();
            foreach (var slot in inventorySlots)
            {
                Item? item = null;
                if (slot.ItemId.HasValue)
                {
                    item = await _itemRepository.GetItemById(slot.ItemId.Value);
                }
                var response = new InventorySlotResponse
                {
                    Id = slot.Id,
                    ItemId = slot.ItemId ?? 0,
                    Quantity = slot.Quantity,
                    CollectableType = item != null ? item.CollectableType : "NONE",
                    Icon = item != null ? item.Icon : "NONE",
                    SlotIndex = slot.SlotIndex
                };
                inventorySlotResponses.Add(response);
            }
            return new()
            {
                Code = 200,
                Message = "Inventory slots retrieved successfully.",
                Data = inventorySlotResponses,
            };
        }

    }
}
