using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using KunFarm.BLL.Interfaces;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var responses = await BuildInventorySlotResponse(inventorySlots);
            return new()
            {
                Code = 200,
                Message = "Inventory slots retrieved successfully.",
                Data = responses,
            };
        }

        public async Task<ApiResponse<List<InventorySlotResponse>>> SaveInventory(int playerId, InventorySaveList requests)
        {
            await _inventorySlotRepository.DeleteAllByPlayerId(playerId);
            Console.WriteLine($"----------------------------------Saving inventory for player {playerId} with {requests.Data.Count} slots.");
            foreach (SaveInventoryRequest slot in requests.Data)
            {
                Item? item = await _itemRepository.GetItemByType(slot.CollectableType);
                await _inventorySlotRepository.AddAsync(
                        new()
                        {
                            SlotIndex = slot.SlotIndex,
                            ItemId = item == null ? null : item.Id,
                            PlayerStateId = playerId,
                            Quantity = slot.Quantity,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
            }
            var inventorySlots = await _inventorySlotRepository.GetAllByPlayer(playerId);
            var responses = await BuildInventorySlotResponse(inventorySlots);
            Console.WriteLine($"----------------------------------Inventory for player {playerId} saved successfully with {responses.Count} slots.");
            return new()
            {
                Code = 200,
                Message = "Inventory slots retrieved successfully.",
                Data = responses,
            };
        }

        private async Task<List<InventorySlotResponse>> BuildInventorySlotResponse(List<InventorySlot> inventorySlots)
        {
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
            return inventorySlotResponses;
        }

        public async Task<ApiResponse<InventorySlotResponse>> UpdateInventorySlot(int playerId, UpdateInventorySlotRequest request)
        {
            // Tìm slot hiện tại
            var existingSlot = await _inventorySlotRepository.GetByPlayerAndSlotIndex(playerId, request.SlotIndex);
            
            // Tìm item theo type
            Item? item = await _itemRepository.GetItemByType(request.CollectableType);
            
            if (existingSlot != null)
            {
                // Update slot hiện tại
                existingSlot.ItemId = item?.Id;
                existingSlot.Quantity = request.Quantity;
                existingSlot.UpdatedAt = DateTime.UtcNow;
                
                await _inventorySlotRepository.UpdateAsync(existingSlot);
                Console.WriteLine($"Updated slot {request.SlotIndex} for player {playerId}: {request.CollectableType} x{request.Quantity}");
            }
            else
            {
                // Tạo slot mới
                var newSlot = new InventorySlot
                {
                    SlotIndex = request.SlotIndex,
                    ItemId = item?.Id,
                    PlayerStateId = playerId,
                    Quantity = request.Quantity,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                existingSlot = await _inventorySlotRepository.AddAsync(newSlot);
                Console.WriteLine($"Created new slot {request.SlotIndex} for player {playerId}: {request.CollectableType} x{request.Quantity}");
            }
            
            // Tạo response
            var response = new InventorySlotResponse
            {
                Id = existingSlot.Id,
                ItemId = existingSlot.ItemId ?? 0,
                Quantity = existingSlot.Quantity,
                CollectableType = item?.CollectableType ?? "NONE",
                Icon = item?.Icon ?? "NONE",
                SlotIndex = existingSlot.SlotIndex
            };
            
            return new ApiResponse<InventorySlotResponse>
            {
                Code = 200,
                Message = "Inventory slot updated successfully.",
                Data = response
            };
        }

        public async Task<ApiResponse<List<InventorySlotResponse>>> BatchUpdateInventory(int playerId, BatchUpdateInventoryRequest request)
        {
            Console.WriteLine($"Batch updating inventory for player {playerId} with {request.Slots.Count} slots");
            
            var updatedSlots = new List<InventorySlot>();
            
            foreach (var slotRequest in request.Slots)
            {
                // Tìm slot hiện tại
                var existingSlot = await _inventorySlotRepository.GetByPlayerAndSlotIndex(playerId, slotRequest.SlotIndex);
                
                // Tìm item theo type (chấp nhận null cho NONE)
                Item? item = null;
                if (slotRequest.CollectableType != "NONE")
                {
                    item = await _itemRepository.GetItemByType(slotRequest.CollectableType);
                }
                
                if (existingSlot != null)
                {
                    // Update slot hiện tại
                    existingSlot.ItemId = item?.Id; // null cho NONE slots
                    existingSlot.Quantity = slotRequest.Quantity;
                    existingSlot.UpdatedAt = DateTime.UtcNow;
                    
                    await _inventorySlotRepository.UpdateAsync(existingSlot);
                    updatedSlots.Add(existingSlot);
                    Console.WriteLine($"Updated slot {slotRequest.SlotIndex}: {slotRequest.CollectableType} x{slotRequest.Quantity}");
                }
                else
                {
                    // Tạo slot mới
                    var newSlot = new InventorySlot
                    {
                        SlotIndex = slotRequest.SlotIndex,
                        ItemId = item?.Id, // null cho NONE slots
                        PlayerStateId = playerId,
                        Quantity = slotRequest.Quantity,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    var createdSlot = await _inventorySlotRepository.AddAsync(newSlot);
                    updatedSlots.Add(createdSlot);
                    Console.WriteLine($"Created slot {slotRequest.SlotIndex}: {slotRequest.CollectableType} x{slotRequest.Quantity}");
                }
            }
            
            // Build response
            var responses = await BuildInventorySlotResponse(updatedSlots);
            
            return new ApiResponse<List<InventorySlotResponse>>
            {
                Code = 200,
                Message = $"Batch updated {request.Slots.Count} inventory slots successfully.",
                Data = responses
            };
        }

        public async Task<ApiResponse<bool>> InitInventory(int playerId)
        {
            InventorySlot inventorySlot0 = new()
            {
                SlotIndex = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ItemId = 3,
                Quantity = 2,
                PlayerStateId = playerId,
            };
            await _inventorySlotRepository.AddAsync(inventorySlot0);
            InventorySlot inventorySlot1 = new()
            {
                SlotIndex = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                ItemId = 7,
                Quantity = 10,
                PlayerStateId = playerId,
            };
            await _inventorySlotRepository.AddAsync(inventorySlot1);

            return new ApiResponse<bool>
            {
                Code = 200,
                Data = true
            };
        }

    }
}
