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
    public class PlayerRegularShopSlotRepository : IPlayerRegularShopSlotRepository
    {
        private readonly KunFarmDbContext _context;
      

        public PlayerRegularShopSlotRepository(KunFarmDbContext context)
        {
            _context = context;
        
        }

        public async Task<PlayerRegularShopSlot?> PlayerRegularShopSlot(int palyerId, int shopSlotId)
        {
            return await _context.PlayerRegularShopSlots
                .Where(s => s.PlayerStateId == palyerId && s.RegularShopSlotId == shopSlotId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(PlayerRegularShopSlot entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

    }
}
