using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Entities
{
    public class InventorySlot : BaseEntity
    {
        public int SlotIndex { get; set; }
        public int Quantity { get; set; }

        public int PlayerStateId { get; set; }
        public PlayerState PlayerState { get; set; } = null!;

        public int? ItemId { get; set; }
        public Item? Item { get; set; }
    }
}
