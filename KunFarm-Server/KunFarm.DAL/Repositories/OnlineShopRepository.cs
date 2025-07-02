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
    public class OnlineShopRepository : BaseRepository<OnlineShopSlot>, IOnlineShopRepository
    {
        public OnlineShopRepository(KunFarmDbContext context) : base(context)
        {
        }

        public async Task<List<OnlineShopSlot>> GetAllWithoutSellerId(int playerId)
        {
            return await _context.OnlineShopSlots
                .Where(o => o.SellerId != playerId && o.CanBuy == true && o.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<List<OnlineShopSlot>> GetBySellerId(int sellerId)
        {
            return await _context.OnlineShopSlots
                .Where(slot => slot.SellerId == sellerId && !slot.IsDeleted)
                .OrderByDescending(slot => slot.CreatedAt)
                .ToListAsync();
        }
    }
}
