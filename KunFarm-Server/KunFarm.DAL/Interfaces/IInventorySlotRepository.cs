using KunFarm.DAL.Entities;
using KunFarm.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Interfaces
{
    public interface IInventorySlotRepository : IBaseRepository<InventorySlot>
    {

        Task<List<InventorySlot>> GetAllByPlayer(int playerId);
        Task DeleteAllByPlayerId(int playerId);
        Task<InventorySlot?> GetByPlayerAndSlotIndex(int playerId, int slotIndex);

    }
}
