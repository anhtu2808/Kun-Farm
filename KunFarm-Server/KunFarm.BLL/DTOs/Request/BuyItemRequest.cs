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
    }
}
