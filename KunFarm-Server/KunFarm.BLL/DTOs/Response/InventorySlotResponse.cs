using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.DTOs.Response
{
    public class InventorySlotResponse
    {
        public int Id { get; set; }
        public int SlotIndex { get; set; }

        public int ItemId { get; set; }
        public string CollectableType { get; set; }
        public string Icon { get; set; }

        public int Quantity { get; set; }

    }
}
