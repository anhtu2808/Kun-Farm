using KunFarm.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Interfaces
{
    public interface IPlayerRegularShopSlotRepository
    {
        Task<PlayerRegularShopSlot?> PlayerRegularShopSlot(int palyerId, int shopSlotId);
        Task UpdateAsync(PlayerRegularShopSlot playerRegularShopSlot);
        Task CreateAsync(PlayerRegularShopSlot playerRegularShopSlot);
    }
}
