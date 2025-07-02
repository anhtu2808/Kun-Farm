using KunFarm.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Interfaces
{
    public interface IOnlineShopRepository : IBaseRepository<OnlineShopSlot>
    {

        Task<List<OnlineShopSlot>> GetAllWithoutSellerId(int playerId);

    }
}
