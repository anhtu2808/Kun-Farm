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
    }
}
