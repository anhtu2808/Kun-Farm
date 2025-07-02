using KunFarm.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Interfaces
{
    public interface IItemRepository
    {
        Task<Item?> GetItemById(int id);
        Task<Item?> GetItemByType(string type);
    }
}
