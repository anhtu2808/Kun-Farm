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
    public class RegularShopSlotRepository : IRegularShopSlotRepository
    {
        private readonly KunFarmDbContext _context;

        public RegularShopSlotRepository(KunFarmDbContext context)
        {
            _context = context;
        }

        public async Task<List<RegularShopSlot>> GetAllSlot()
        {
            return await _context.RegularShopSlots
                .Include(s => s.Item)
                .ToListAsync(); 
        }
    }
}
