using KunFarm.DAL.Data;
using KunFarm.DAL.Entities;
using KunFarm.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Repositories
{
    public class InventorySlotRepository : BaseRepository<Entities.InventorySlot>, IInventorySlotRepository
    {
        public InventorySlotRepository(KunFarmDbContext context) : base(context)
        {
        }

        public async Task DeleteAllByPlayerId(int playerId)
        {
            var inventorySlots = await _context.InventorySlots
                .Where(s => s.PlayerStateId == playerId)
                .ToListAsync();

            _context.InventorySlots.RemoveRange(inventorySlots);
            await _context.SaveChangesAsync();

        }

        public async Task<List<InventorySlot>> GetAllByPlayer(int playerId)
        {
            return await _context.InventorySlots
                .Where(slot => slot.PlayerStateId == playerId)
                .OrderBy(slot => slot.SlotIndex)
                .ToListAsync();
        }

        public async Task<InventorySlot?> GetByPlayerAndSlotIndex(int playerId, int slotIndex)
        {
            return await _context.InventorySlots
                .FirstOrDefaultAsync(slot => slot.PlayerStateId == playerId && slot.SlotIndex == slotIndex);
        }

    }
}
