using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.DTOs.Response
{
    public class SellItemResponse
    {
        public int Id { get; set; }
        public string CollectableType { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public string Icon { get; set; }

        public bool CanBuy { get; set; }

    }
}
