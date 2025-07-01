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
    public class ItemRepository : IItemRepository
    {
        private readonly KunFarmDbContext _context;

        public ItemRepository(KunFarmDbContext context)
        {
            _context = context;
        }

        public async Task<Item?> GetItemById(int id)
        {
            return await _context.Items
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
