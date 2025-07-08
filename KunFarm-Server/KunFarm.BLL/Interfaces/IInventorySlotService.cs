using KunFarm.BLL.DTOs.Request;
using KunFarm.BLL.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.Interfaces
{
    public interface IInventorySlotService
    {
        Task<ApiResponse<List<InventorySlotResponse>>> GetInventorySlots(int playerId);

        Task<ApiResponse<List<InventorySlotResponse>>> SaveInventory(int playerId, InventorySaveList requests);
        
        Task<ApiResponse<InventorySlotResponse>> UpdateInventorySlot(int playerId, UpdateInventorySlotRequest request);
        
        Task<ApiResponse<List<InventorySlotResponse>>> BatchUpdateInventory(int playerId, BatchUpdateInventoryRequest request);

        Task<ApiResponse<bool>> InitInventory(int playerId);
    }
}
