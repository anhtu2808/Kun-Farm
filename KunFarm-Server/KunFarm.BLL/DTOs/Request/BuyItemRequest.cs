using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.DTOs.Request
{
    public class BuyItemRequest
    {
        public int SlotId { get; set; }

        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
    }

    public class BuyItemRequestList
    {
        public List<BuyItemRequest> Items { get; set; } = new();
    }
}
