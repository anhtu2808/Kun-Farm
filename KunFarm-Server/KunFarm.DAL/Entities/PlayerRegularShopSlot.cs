using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Entities
{
    public class PlayerRegularShopSlot
    {
        // Khóa ngoại về PlayerState
        public int PlayerStateId { get; set; }
        public PlayerState PlayerState { get; set; } = null!;

        // Khóa ngoại về RegularShopSlot
        public int RegularShopSlotId { get; set; }
        public RegularShopSlot RegularShopSlot { get; set; } = null!;

        public int? CurrentStock { get; set; }

    }
}
