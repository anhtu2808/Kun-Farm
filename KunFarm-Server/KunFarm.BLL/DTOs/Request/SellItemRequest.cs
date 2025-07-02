using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.DTOs.Request
{
    public class SellItemRequest
    {
        public string CollectableType { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }

    }
}
