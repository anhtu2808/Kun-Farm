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

        public async Task<List<InventorySlot>> GetAllByPlayer(int playerId)
        {
            return await _context.InventorySlots
                .Where(slot => slot.PlayerStateId == playerId)
                .ToListAsync();
        }

    }
}
