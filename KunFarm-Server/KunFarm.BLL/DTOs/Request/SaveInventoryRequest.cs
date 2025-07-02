using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.DTOs.Request
{
    public class SaveInventoryRequest
    {
        public int SlotIndex { get; set; }
        public string CollectableType { get; set; }
        public int Quantity { get; set; }
    }

    public class InventorySaveList
    {
        public List<SaveInventoryRequest> Data { get; set; }
    }
}
